using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.PaymentDTO;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BookingSystem.Application.DTOs.PaymentGatewayDTO;
using BookingSystem.Application.Factories;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Application.DTOs.AmenityDTO;

namespace BookingSystem.Application.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IBookingRepository _bookingRepository;
		private readonly UserManager<User> _userManager;
		private readonly IPaymentGatewayFactory _paymentGatewayFactory;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<PaymentService> _logger;
		private readonly IEmailService _emailService;

		public PaymentService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IPaymentRepository paymentRepository,
			IBookingRepository bookingRepository,
			UserManager<User> userManager,
			IPaymentGatewayFactory paymentGatewayFactory,
			IHttpContextAccessor httpContextAccessor,
			IEmailService emailService,
			ILogger<PaymentService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_paymentRepository = paymentRepository;
			_bookingRepository = bookingRepository;
			_userManager = userManager;
			_paymentGatewayFactory = paymentGatewayFactory;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
			_logger = logger;
		}

		private string GetIpAddress()
		{
			var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
			if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
			{
				ipAddress = "127.0.0.1";
			}
			return ipAddress ?? "127.0.0.1";
		}

		public async Task<PagedResult<PaymentDto>> GetPaymentsByHostIdAsync(int hostId, PaymentFilter filter)
		{
			_logger.LogInformation("Fetching payments for homestays owned by host {HostId}.", hostId);

			// Kiểm tra user tồn tại
			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			// Kiểm tra quyền: phải là Host hoặc Admin
			var roles = await _userManager.GetRolesAsync(host);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				throw new BadRequestException("User does not have Host or Admin role.");
			}

			// Lấy danh sách payment theo host
			var pagedPayments = await _paymentRepository.GetPaymentsByHostIdAsync(hostId, filter);
			var paymentDtos = _mapper.Map<List<PaymentDto>>(pagedPayments.Items);

			_logger.LogInformation("Retrieved {Count} payments for host {HostId}.", paymentDtos.Count, hostId);

			return new PagedResult<PaymentDto>
			{
				Items = paymentDtos,
				TotalCount = pagedPayments.TotalCount,
				PageNumber = pagedPayments.PageNumber,
				PageSize = pagedPayments.PageSize,
				TotalPages = (int)Math.Ceiling((double)pagedPayments.TotalCount / pagedPayments.PageSize)
			};
		}

		public async Task<PagedResult<PaymentDto>> GetAllPaymentAsync(PaymentFilter paymentFilter, int? userId = null)
		{
			var payments = await _paymentRepository.GetAllPaymentAsync(paymentFilter, userId);
			var paymentDtos = _mapper.Map<List<PaymentDto>>(payments.Items);

			_logger.LogInformation("Retrieved {Count} payments.", paymentDtos.Count);

			return new PagedResult<PaymentDto>
			{
				Items = paymentDtos,
				TotalCount = payments.TotalCount,
				PageNumber = payments.PageNumber,
				PageSize = payments.PageSize,
				TotalPages = payments.TotalPages
			};
		}

		public async Task<PaymentUrlResponseDto> CreateOnlinePaymentAsync(int userId, CreateOnlinePaymentDto request)
		{
			_logger.LogInformation("Creating online payment for booking {BookingId} by user {UserId}",
				request.BookingId, userId);

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(request.BookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {request.BookingId} not found.");
			}

			if (booking.GuestId != userId)
			{
				throw new BadRequestException("You can only make payments for your own bookings.");
			}

			if (booking.BookingStatus == BookingStatus.Cancelled || booking.BookingStatus == BookingStatus.Rejected)
			{
				throw new BadRequestException("Cannot make payment for cancelled or rejected bookings.");
			}

			// Calculate remaining amount
			var totalPaid = booking.Payments
				.Where(p => p.PaymentStatus == PaymentStatus.Completed)
				.Sum(p => p.PaymentAmount);

			var remainingAmount = booking.TotalAmount - totalPaid;

			if (remainingAmount <= 0)
			{
				throw new BadRequestException("Booking is already fully paid.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Create payment record
				var payment = new Payment
				{
					BookingId = request.BookingId,
					PaymentAmount = remainingAmount,
					PaymentMethod = request.PaymentMethod,
					PaymentStatus = PaymentStatus.Pending,
					PaymentGateway = request.PaymentMethod.ToString(),
					PaymentNotes = request.PaymentNotes,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _paymentRepository.AddAsync(payment);
				await _paymentRepository.SaveChangesAsync();

				// Get payment gateway service
				var gatewayService = _paymentGatewayFactory.GetPaymentGateway(request.PaymentMethod);

				// Create payment URL
				var gatewayRequest = new PaymentGatewayRequest
				{
					BookingId = request.BookingId,
					Amount = remainingAmount,
					OrderInfo = $"Thanh toan dat phong #{booking.BookingCode}",
					ReturnUrl = request.ReturnUrl,
					IpAddress = GetIpAddress()
				};

				var gatewayResponse = await gatewayService.CreatePaymentUrlAsync(gatewayRequest);

				if (!gatewayResponse.Success)
				{
					throw new BadRequestException($"Failed to create payment URL: {gatewayResponse.Message}");
				}

				// Update payment with transaction reference
				payment.TransactionId = gatewayResponse.TransactionId;
				payment.PaymentStatus = PaymentStatus.Processing;
				payment.UpdatedAt = DateTime.UtcNow;
				_paymentRepository.Update(payment);
				await _paymentRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Online payment {PaymentId} created successfully for booking {BookingId}",
					payment.Id, request.BookingId);

				return new PaymentUrlResponseDto
				{
					PaymentId = payment.Id,
					PaymentUrl = gatewayResponse.PaymentUrl,
					TransactionReference = gatewayResponse.TransactionId,
					ExpiryTime = DateTime.UtcNow.AddMinutes(15)
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating online payment for booking {BookingId}", request.BookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<PaymentDto> ProcessPaymentCallbackAsync(PaymentMethod paymentMethod, Dictionary<string, string> callbackData)
		{
			_logger.LogInformation("Processing payment callback for {PaymentMethod}", paymentMethod);

			try
			{
				var gatewayService = _paymentGatewayFactory.GetPaymentGateway(paymentMethod);
				var callbackResult = await gatewayService.ProcessCallbackAsync(callbackData);

				if (!int.TryParse(callbackResult.OrderId, out var bookingId))
				{
					throw new BadRequestException("Invalid booking ID in callback");
				}

				var payments = await _paymentRepository.GetByBookingIdAsync(bookingId);

				// TÌM PAYMENT ĐÃ COMPLETED VỚI CÙNG TRANSACTION ID
				var completedPayment = payments
					.Where(p => p.PaymentStatus == PaymentStatus.Completed &&
							   p.TransactionId == callbackResult.TransactionId)
					.FirstOrDefault();

				if (completedPayment != null)
				{
					_logger.LogInformation("Payment {PaymentId} with transaction {TransactionId} already processed",
						completedPayment.Id, callbackResult.TransactionId);
					return _mapper.Map<PaymentDto>(completedPayment);
				}

				// TÌM PENDING/PROCESSING PAYMENT
				var payment = payments
					.Where(p => p.PaymentStatus == PaymentStatus.Processing ||
							   p.PaymentStatus == PaymentStatus.Pending)
					.OrderByDescending(p => p.CreatedAt)
					.FirstOrDefault();

				if (payment == null)
				{
					_logger.LogWarning("No pending payment found for booking {BookingId}, transaction {TransactionId}",
						bookingId, callbackResult.TransactionId);
					throw new NotFoundException($"No pending payment found for booking {bookingId}");
				}

				await _unitOfWork.BeginTransactionAsync();

				if (callbackResult.Success)
				{
					payment.PaymentStatus = PaymentStatus.Completed;
					payment.TransactionId = callbackResult.TransactionId;
					payment.PaymentGatewayId = callbackResult.TransactionId;
					payment.ProcessedAt = callbackResult.TransactionDate;
					payment.PaymentNotes = (payment.PaymentNotes ?? "") +
						$"\nBank: {callbackResult.BankCode}, Response: {callbackResult.Message}";
					payment.UpdatedAt = DateTime.UtcNow;

					_paymentRepository.Update(payment);
					await _paymentRepository.SaveChangesAsync();

					var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
					if (booking != null)
					{
						var allPayments = await _paymentRepository.GetByBookingIdAsync(bookingId);
						var totalPaid = allPayments
							.Where(p => p.PaymentStatus == PaymentStatus.Completed)
							.Sum(p => p.PaymentAmount);

						_logger.LogInformation("Booking {BookingId} payment status: {TotalPaid}/{TotalAmount}",
							bookingId, totalPaid, booking.TotalAmount);

						if (totalPaid >= booking.TotalAmount && booking.BookingStatus == BookingStatus.Pending)
						{
							if (booking.Homestay.IsInstantBook)
							{
								booking.BookingStatus = BookingStatus.Confirmed;
								booking.UpdatedAt = DateTime.UtcNow;
								_bookingRepository.Update(booking);
								await _bookingRepository.SaveChangesAsync();

								_logger.LogInformation("Booking {BookingId} auto-confirmed after successful payment", bookingId);
							}
						}
					}

					if (callbackResult.Success && booking != null)
					{
						_ = _emailService.SendPaymentSuccessAsync(
							booking.Guest.Email!,
							booking.Guest.FullName,
							booking.BookingCode,
							payment.PaymentAmount,
							payment.PaymentMethod.ToString()
						);
					}
				}
				else
				{
					payment.PaymentStatus = PaymentStatus.Failed;
					payment.FailureReason = callbackResult.Message;
					payment.PaymentNotes = (payment.PaymentNotes ?? "") +
						$"\nFailed: {callbackResult.Message}, Code: {callbackResult.ResponseCode}";
					payment.UpdatedAt = DateTime.UtcNow;

					_paymentRepository.Update(payment);
					await _paymentRepository.SaveChangesAsync();

					var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
					if (booking != null)
					{
						_ = _emailService.SendPaymentFailedAsync(
							booking.Guest.Email!,
							booking.Guest.FullName,
							booking.BookingCode,
							callbackResult.Message
						);
					}
					_logger.LogWarning("Payment {PaymentId} failed: {Reason}", payment.Id, callbackResult.Message);
				}

				await _unitOfWork.CommitTransactionAsync();
				

				return _mapper.Map<PaymentDto>(payment);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing payment callback for {PaymentMethod}", paymentMethod);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<PaymentDto> CreatePaymentAsync(int userId, CreatePaymentDto request)
		{
			_logger.LogInformation("Creating manual payment for booking {BookingId} by user {UserId}.",
				request.BookingId, userId);

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(request.BookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {request.BookingId} not found.");
			}

			if (booking.GuestId != userId)
			{
				throw new BadRequestException("You can only make payments for your own bookings.");
			}

			if (booking.BookingStatus == BookingStatus.Cancelled || booking.BookingStatus == BookingStatus.Rejected)
			{
				throw new BadRequestException("Cannot make payment for cancelled or rejected bookings.");
			}

			var totalPaid = booking.Payments
				.Where(p => p.PaymentStatus == PaymentStatus.Completed)
				.Sum(p => p.PaymentAmount);

			var remainingAmount = booking.TotalAmount - totalPaid;

			if (request.PaymentAmount > remainingAmount)
			{
				throw new BadRequestException($"Payment amount exceeds remaining balance of {remainingAmount:C}.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var payment = new Payment
				{
					BookingId = request.BookingId,
					PaymentAmount = request.PaymentAmount,
					PaymentMethod = request.PaymentMethod,
					PaymentStatus = PaymentStatus.Pending,
					PaymentNotes = request.PaymentNotes,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _paymentRepository.AddAsync(payment);
				await _paymentRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Payment {PaymentId} created successfully for booking {BookingId}.",
					payment.Id, request.BookingId);

				return _mapper.Map<PaymentDto>(payment);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating payment.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto request)
		{
			_logger.LogInformation("Processing payment {PaymentId}.", request.PaymentId);

			var payment = await _paymentRepository.GetByIdWithDetailsAsync(request.PaymentId);
			if (payment == null)
			{
				throw new NotFoundException($"Payment with ID {request.PaymentId} not found.");
			}

			// KIỂM TRA NẾU PAYMENT ĐÃ ĐƯỢC XỬ LÝ THÀNH CÔNG RỒI
			if (payment.PaymentStatus == PaymentStatus.Completed)
			{
				_logger.LogInformation("Payment {PaymentId} is already completed. Returning existing result.", request.PaymentId);
				return _mapper.Map<PaymentDto>(payment);
			}

			// KIỂM TRA NẾU PAYMENT ĐÃ FAILED HOẶC REFUNDED
			if (payment.PaymentStatus == PaymentStatus.Failed ||
				payment.PaymentStatus == PaymentStatus.Refunded ||
				payment.PaymentStatus == PaymentStatus.PartiallyRefunded)
			{
				throw new BadRequestException($"Cannot process payment with status {payment.PaymentStatus}.");
			}

			// CHỈ XỬ LÝ NẾU ĐANG Ở TRẠNG THÁI PENDING HOẶC PROCESSING
			if (payment.PaymentStatus != PaymentStatus.Pending &&
				payment.PaymentStatus != PaymentStatus.Processing)
			{
				throw new BadRequestException("Only pending or processing payments can be processed.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Cập nhật thông tin payment
				payment.PaymentStatus = PaymentStatus.Completed;
				payment.TransactionId = request.TransactionId;
				payment.PaymentGatewayId = request.PaymentGatewayId;
				payment.PaymentGateway = request.PaymentGateway;
				payment.ProcessedAt = DateTime.UtcNow;
				payment.UpdatedAt = DateTime.UtcNow;

				_paymentRepository.Update(payment);
				await _paymentRepository.SaveChangesAsync();

				// Kiểm tra và cập nhật trạng thái booking
				var booking = payment.Booking;

				// QUAN TRỌNG: Refresh lại danh sách payments từ DB để đảm bảo tính chính xác
				var allPayments = await _paymentRepository.GetByBookingIdAsync(booking.Id);
				var totalPaid = allPayments
					.Where(p => p.PaymentStatus == PaymentStatus.Completed)
					.Sum(p => p.PaymentAmount);

				_logger.LogInformation("Booking {BookingId}: Total amount = {TotalAmount}, Total paid = {TotalPaid}",
					booking.Id, booking.TotalAmount, totalPaid);

				if (totalPaid >= booking.TotalAmount && booking.BookingStatus == BookingStatus.Pending)
				{
					if (booking.Homestay.IsInstantBook)
					{
						booking.BookingStatus = BookingStatus.Confirmed;
						booking.UpdatedAt = DateTime.UtcNow;
						_bookingRepository.Update(booking);
						await _bookingRepository.SaveChangesAsync();

						_logger.LogInformation("Booking {BookingId} auto-confirmed after full payment.", booking.Id);
					}
					else
					{
						_logger.LogInformation("Booking {BookingId} is fully paid but requires manual confirmation by host.", booking.Id);
					}
				}
				else if (totalPaid < booking.TotalAmount)
				{
					_logger.LogInformation("Booking {BookingId} is partially paid. Remaining: {Remaining}",
						booking.Id, booking.TotalAmount - totalPaid);
				}

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Payment {PaymentId} processed successfully.", request.PaymentId);

				return _mapper.Map<PaymentDto>(payment);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while processing payment {PaymentId}.", request.PaymentId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<RefundStatusDto> GetRefundStatusAsync(int paymentId)
		{
			var payment = await _paymentRepository.GetByIdWithDetailsAsync(paymentId);
			if (payment == null)
			{
				throw new NotFoundException($"Payment with ID {paymentId} not found.");
			}

			var refundedAmount = payment.RefundAmount ?? 0;
			var refundableAmount = payment.PaymentAmount - refundedAmount;

			return new RefundStatusDto
			{
				PaymentId = payment.Id,
				OriginalAmount = payment.PaymentAmount,
				RefundedAmount = refundedAmount,
				RefundableAmount = refundableAmount,
				CanRefund = payment.PaymentStatus == PaymentStatus.Completed && refundableAmount > 0
			};
		}

		public async Task<PaymentDto> RefundPaymentAsync(int paymentId, int userId, RefundPaymentDto request)
		{
			_logger.LogInformation("Refunding payment {PaymentId} by user {UserId}.", paymentId, userId);

			var payment = await _paymentRepository.GetByIdWithDetailsAsync(paymentId);
			if (payment == null)
			{
				throw new NotFoundException($"Payment with ID {paymentId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isHost = roles.Contains("Host") && payment.Booking.Homestay.OwnerId == userId;

			if (!isAdmin && !isHost)
			{
				throw new BadRequestException("You do not have permission to refund this payment.");
			}

			if (payment.PaymentStatus != PaymentStatus.Completed)
			{
				throw new BadRequestException("Only completed payments can be refunded.");
			}

			if (payment.RefundAmount.HasValue && payment.RefundAmount.Value > 0)
			{
				var remainingRefundable = payment.PaymentAmount - payment.RefundAmount.Value;
				if (request.RefundAmount > remainingRefundable)
				{
					throw new BadRequestException($"Refund amount exceeds remaining refundable amount of {remainingRefundable:C}.");
				}
			}

			if (request.RefundAmount > payment.PaymentAmount)
			{
				throw new BadRequestException($"Refund amount cannot exceed payment amount of {payment.PaymentAmount:C}.");
			}

			

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var alreadyRefunded = payment.RefundAmount ?? 0;
				var totalRefund = alreadyRefunded + request.RefundAmount;

				if (totalRefund > payment.PaymentAmount)
				{
					throw new BadRequestException("Total refund amount exceeds payment amount.");
				}

				// Process refund through payment gateway if it's an online payment
				if (payment.PaymentMethod == PaymentMethod.VNPay ||
					payment.PaymentMethod == PaymentMethod.ZaloPay ||
					payment.PaymentMethod == PaymentMethod.Momo)
				{
					try
					{
						var gatewayService = _paymentGatewayFactory.GetPaymentGateway(payment.PaymentMethod);
						var refundResult = await gatewayService.RefundAsync(payment.TransactionId!, request.RefundAmount);

						if (!refundResult.Success)
						{
							_logger.LogWarning("Gateway refund failed: {Message}. Recording manual refund.", refundResult.Message);
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error processing gateway refund. Recording manual refund.");
					}
				}

				payment.RefundAmount = totalRefund;
				payment.RefundedAt = DateTime.UtcNow;
				payment.PaymentNotes = (payment.PaymentNotes ?? "") + $"\nRefund {DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.RefundAmount:C} - {request.RefundReason}";
				payment.UpdatedAt = DateTime.UtcNow;

				if (totalRefund >= payment.PaymentAmount)
				{
					payment.PaymentStatus = PaymentStatus.Refunded;
				}
				else
				{
					payment.PaymentStatus = PaymentStatus.PartiallyRefunded;
				}

				_paymentRepository.Update(payment);
				await _paymentRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();

				_ = _emailService.SendRefundProcessedAsync(
					payment.Booking.Guest.Email!,
					payment.Booking.Guest.FullName,
					payment.Booking.BookingCode,
					request.RefundAmount
				);
				_logger.LogInformation("Payment {PaymentId} refunded successfully.", paymentId);

				return _mapper.Map<PaymentDto>(payment);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while refunding payment {PaymentId}.", paymentId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<PaymentDto?> GetByIdAsync(int paymentId)
		{
			var payment = await _paymentRepository.GetByIdWithDetailsAsync(paymentId);
			if (payment == null)
			{
				throw new NotFoundException($"Payment with ID {paymentId} not found.");
			}

			return _mapper.Map<PaymentDto>(payment);
		}

		public async Task<IEnumerable<PaymentDto>> GetByBookingIdAsync(int bookingId)
		{
			var payments = await _paymentRepository.GetByBookingIdAsync(bookingId);
			return _mapper.Map<IEnumerable<PaymentDto>>(payments);
		}

		public async Task<bool> MarkPaymentAsFailedAsync(int paymentId, string failureReason)
		{
			_logger.LogInformation("Marking payment {PaymentId} as failed.", paymentId);

			var payment = await _paymentRepository.GetByIdAsync(paymentId);
			if (payment == null)
			{
				throw new NotFoundException($"Payment with ID {paymentId} not found.");
			}

			if (payment.PaymentStatus != PaymentStatus.Pending && payment.PaymentStatus != PaymentStatus.Processing)
			{
				throw new BadRequestException("Only pending or processing payments can be marked as failed.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				payment.PaymentStatus = PaymentStatus.Failed;
				payment.FailureReason = failureReason;
				payment.UpdatedAt = DateTime.UtcNow;

				_paymentRepository.Update(payment);
				await _paymentRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Payment {PaymentId} marked as failed.", paymentId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while marking payment {PaymentId} as failed.", paymentId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}
	}
}