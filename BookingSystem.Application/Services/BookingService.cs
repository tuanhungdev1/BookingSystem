using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.BookingDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.ServiceUpdate
{
	public class BookingService : IBookingService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IBookingRepository _bookingRepository;
		private readonly IHomestayRepository _homestayRepository;
		private readonly IAvailabilityCalendarRepository _availabilityCalendarRepository;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<BookingService> _logger;
		private readonly IEmailService _emailService;
		private readonly ICouponUsageRepository _couponUsageRepository;

		private const decimal CLEANING_FEE_PERCENTAGE = 0.05m; // 5% of base amount
		private const decimal SERVICE_FEE_PERCENTAGE = 0.10m;  // 10% of base amount
		private const decimal TAX_PERCENTAGE = 0.08m;           // 8% VAT
		private const decimal WEEKLY_DISCOUNT_THRESHOLD = 7;    // 7 nights
		private const decimal MONTHLY_DISCOUNT_THRESHOLD = 30;   // 30 nights

		public BookingService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IBookingRepository bookingRepository,
			IHomestayRepository homestayRepository,
			IAvailabilityCalendarRepository availabilityCalendarRepository,
			UserManager<User> userManager,
			IEmailService emailService,
			ICouponUsageRepository couponUsageRepository,
			ILogger<BookingService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_bookingRepository = bookingRepository;
			_homestayRepository = homestayRepository;
			_availabilityCalendarRepository = availabilityCalendarRepository;
			_userManager = userManager;
			_logger = logger;
			_couponUsageRepository = couponUsageRepository;
			_emailService = emailService;
		}

		private string GenerateBookingCode()
		{
			// Format: BK-YYYYMMDD-XXXXX (e.g., BK-20250107-A1B2C)
			var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
			var randomPart = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();
			return $"BK-{datePart}-{randomPart}";
		}

		public async Task<BookingPriceBreakdownDto> CalculateBookingPriceAsync(BookingPriceCalculationDto request)
		{
			_logger.LogInformation("Calculating booking price for homestay {HomestayId} from {CheckIn} to {CheckOut}.",
				request.HomestayId, request.CheckInDate, request.CheckOutDate);

			// Validate dates
			if (request.CheckOutDate <= request.CheckInDate)
			{
				throw new BadRequestException("Check-out date must be after check-in date.");
			}

			var homestay = await _homestayRepository.GetByIdAsync(request.HomestayId);
			if (homestay == null)
			{
				throw new NotFoundException($"Homestay with ID {request.HomestayId} not found.");
			}

			if (!homestay.IsActive || !homestay.IsApproved)
			{
				throw new BadRequestException("This homestay is not available for booking.");
			}

			// Calculate number of nights
			var numberOfNights = (request.CheckOutDate.Date - request.CheckInDate.Date).Days;

			if (numberOfNights < homestay.MinimumNights)
			{
				throw new BadRequestException($"Minimum stay is {homestay.MinimumNights} night(s).");
			}

			if (numberOfNights > homestay.MaximumNights)
			{
				throw new BadRequestException($"Maximum stay is {homestay.MaximumNights} night(s).");
			}

			if (request.NumberOfGuests > homestay.MaximumGuests)
			{
				throw new BadRequestException($"Maximum number of guests is {homestay.MaximumGuests}.");
			}

			// Get availability calendar for the date range
			var startDate = DateOnly.FromDateTime(request.CheckInDate);
			var endDate = DateOnly.FromDateTime(request.CheckOutDate).AddDays(-1); // Don't include checkout date
			var calendars = await _availabilityCalendarRepository.GetByDateRangeAsync(
				request.HomestayId, startDate, endDate);

			var calendarDict = calendars.ToDictionary(c => c.AvailableDate);

			// Calculate nightly prices
			var nightlyPrices = new List<NightlyPriceDto>();
			var baseAmount = 0m;
			var currentDate = startDate;

			while (currentDate < DateOnly.FromDateTime(request.CheckOutDate))
			{
				decimal nightPrice;
				bool isCustomPrice = false;
				bool isWeekend = false;

				// Check if there's a custom price in the calendar
				if (calendarDict.ContainsKey(currentDate) && calendarDict[currentDate].CustomPrice.HasValue)
				{
					nightPrice = calendarDict[currentDate].CustomPrice.Value;
					isCustomPrice = true;
				}
				else
				{
					// Check if it's a weekend
					var dayOfWeek = currentDate.DayOfWeek;
					isWeekend = dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Saturday;

					if (isWeekend && homestay.WeekendPrice.HasValue)
					{
						nightPrice = homestay.WeekendPrice.Value;
					}
					else
					{
						nightPrice = homestay.BaseNightlyPrice;
					}
				}

				nightlyPrices.Add(new NightlyPriceDto
				{
					Date = currentDate,
					Price = nightPrice,
					IsWeekend = isWeekend,
					IsCustomPrice = isCustomPrice
				});

				baseAmount += nightPrice;
				currentDate = currentDate.AddDays(1);
			}

			// Calculate discount based on length of stay
			var discountAmount = 0m;
			if (numberOfNights >= MONTHLY_DISCOUNT_THRESHOLD && homestay.MonthlyDiscount.HasValue)
			{
				discountAmount = baseAmount * (homestay.MonthlyDiscount.Value / 100);
			}
			else if (numberOfNights >= WEEKLY_DISCOUNT_THRESHOLD && homestay.WeeklyDiscount.HasValue)
			{
				discountAmount = baseAmount * (homestay.WeeklyDiscount.Value / 100);
			}

			var discountedBaseAmount = baseAmount - discountAmount;

			// Calculate fees
			var cleaningFee = Math.Round(discountedBaseAmount * CLEANING_FEE_PERCENTAGE, 2);
			var serviceFee = Math.Round(discountedBaseAmount * SERVICE_FEE_PERCENTAGE, 2);
			var subtotal = discountedBaseAmount + cleaningFee + serviceFee;
			var taxAmount = Math.Round(subtotal * TAX_PERCENTAGE, 2);
			var totalAmount = subtotal + taxAmount;

			return new BookingPriceBreakdownDto
			{
				NumberOfNights = numberOfNights,
				PricePerNight = baseAmount / numberOfNights,
				BaseAmount = baseAmount,
				CleaningFee = cleaningFee,
				ServiceFee = serviceFee,
				TaxAmount = taxAmount,
				DiscountAmount = discountAmount,
				TotalAmount = totalAmount,
				NightlyPrices = nightlyPrices
			};
		}

		// BookingSystem.Application.Services/BookingService.cs
		public async Task<BookingDto> CreateBookingAsync(int guestId, CreateBookingDto request)
		{
			_logger.LogInformation("Creating booking for homestay {HomestayId} by guest {GuestId}.",
				request.HomestayId, guestId);

			// Validate guest
			var guest = await _userManager.FindByIdAsync(guestId.ToString());
			if (guest == null)
			{
				throw new NotFoundException($"Guest with ID {guestId} not found.");
			}

			var homestay = await _homestayRepository.GetByIdAsync(request.HomestayId);
			if (homestay == null)
			{
				throw new NotFoundException($"Homestay with ID {request.HomestayId} not found.");
			}

			if (homestay.RoomsAtThisPrice <= 0)
			{
				await _unitOfWork.RollbackTransactionAsync();
				_logger.LogWarning(
					"Attempt to book homestay {HomestayId} when RoomsAtThisPrice = 0. Booking blocked.",
					homestay.Id);

				throw new BadRequestException(
					"Rất tiếc, hiện tại đã hết phòng với mức giá này. Vui lòng chọn ngày khác hoặc liên hệ chủ nhà.");
			}

			if (homestay.OwnerId == guestId)
			{
				throw new BadRequestException("Bạn không thể đặt phòng homestay do chính mình sở hữu.");
			}

			var guestRoles = await _userManager.GetRolesAsync(guest);
			if (guestRoles.Contains("Admin"))
			{
				throw new BadRequestException("Quản trị viên không được phép đặt phòng trên hệ thống.");
			}

			// ✅ THÊM: Validate thông tin người ở thực tế nếu đặt cho người khác
			if (request.IsBookingForSomeoneElse)
			{
				if (string.IsNullOrWhiteSpace(request.ActualGuestFullName))
				{
					throw new BadRequestException("Actual guest full name is required when booking for someone else.");
				}

				if (string.IsNullOrWhiteSpace(request.ActualGuestPhoneNumber))
				{
					throw new BadRequestException("Actual guest phone number is required when booking for someone else.");
				}
			}

			// Validate dates
			if (request.CheckOutDate <= request.CheckInDate)
			{
				throw new BadRequestException("Check-out date must be after check-in date.");
			}

			if (request.CheckInDate.Date < DateTime.UtcNow.Date)
			{
				throw new BadRequestException("Check-in date cannot be in the past.");
			}

			// Validate guest numbers
			if (request.NumberOfGuests != request.NumberOfAdults + request.NumberOfChildren)
			{
				throw new BadRequestException("Number of guests must equal adults plus children.");
			}

			if (request.NumberOfAdults < 1)
			{
				throw new BadRequestException("At least one adult is required.");
			}

			// Check if homestay is available
			var isAvailable = await IsHomestayAvailableAsync(
				request.HomestayId, request.CheckInDate, request.CheckOutDate);

			if (!isAvailable)
			{
				throw new BadRequestException("Homestay is not available for the selected dates.");
			}

			// Calculate price
			var priceBreakdown = await CalculateBookingPriceAsync(new BookingPriceCalculationDto
			{
				HomestayId = request.HomestayId,
				CheckInDate = request.CheckInDate,
				CheckOutDate = request.CheckOutDate,
				NumberOfGuests = request.NumberOfGuests
			});

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var booking = new Booking
				{
					BookingCode = GenerateBookingCode(),
					GuestId = guestId,
					HomestayId = request.HomestayId,
					CheckInDate = request.CheckInDate,
					CheckOutDate = request.CheckOutDate,
					NumberOfGuests = request.NumberOfGuests,
					NumberOfAdults = request.NumberOfAdults,
					NumberOfChildren = request.NumberOfChildren,
					NumberOfInfants = request.NumberOfInfants,
					BaseAmount = priceBreakdown.BaseAmount,
					CleaningFee = priceBreakdown.CleaningFee,
					ServiceFee = priceBreakdown.ServiceFee,
					TaxAmount = priceBreakdown.TaxAmount,
					DiscountAmount = priceBreakdown.DiscountAmount,
					TotalAmount = priceBreakdown.TotalAmount,
					BookingStatus = BookingStatus.Pending,
					SpecialRequests = request.SpecialRequests,

					// ✅ THÊM: Lưu thông tin người đặt
					GuestFullName = request.GuestFullName,
					GuestEmail = request.GuestEmail,
					GuestPhoneNumber = request.GuestPhoneNumber,
					GuestAddress = request.GuestAddress,
					GuestCity = request.GuestCity,
					GuestCountry = request.GuestCountry,

					// ✅ THÊM: Lưu thông tin người ở thực tế (nếu có)
					IsBookingForSomeoneElse = request.IsBookingForSomeoneElse,
					ActualGuestFullName = request.ActualGuestFullName,
					ActualGuestEmail = request.ActualGuestEmail,
					ActualGuestPhoneNumber = request.ActualGuestPhoneNumber,
					ActualGuestIdNumber = request.ActualGuestIdNumber,
					ActualGuestNotes = request.ActualGuestNotes,
					// THÊM: Đặt thời gian hết hạn thanh toán là 30 phút từ bây giờ
					PaymentExpiresAt = DateTime.UtcNow.AddMinutes(30),

					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _bookingRepository.AddAsync(booking);
				await _bookingRepository.SaveChangesAsync();
				await BlockDatesForBookingAsync(booking, "Ngày này đã được đặt bởi khách hàng");

				homestay.RoomsAtThisPrice -= 1;
				_homestayRepository.Update(homestay);

				_logger.LogInformation(
					"RoomsAtThisPrice reduced for homestay {HomestayId}: {Old} → {New}",
					homestay.Id,
					homestay.RoomsAtThisPrice + 1,
					homestay.RoomsAtThisPrice);
				await _unitOfWork.CommitTransactionAsync();

				_ = _emailService.SendBookingConfirmationAsync(
					booking.Guest.Email!,
					booking.Guest.FullName,
					booking.BookingCode,
					booking.Homestay.HomestayTitle,
					booking.CheckInDate,
					booking.CheckOutDate,
					booking.TotalAmount
				);

				// ✅ GỬI EMAIL CHO HOST
				var host = await _userManager.FindByIdAsync(booking.Homestay.OwnerId.ToString());
				if (host != null)
				{
					_ = _emailService.SendNewBookingNotificationToHostAsync(
						host.Email!,
						host.FullName,
						booking.BookingCode,
						booking.Guest.FullName,
						booking.Homestay.HomestayTitle,
						booking.CheckInDate,
						booking.CheckOutDate
					);
				}

				_logger.LogInformation("Booking {BookingCode} created successfully{ForSomeoneElse}.",
					booking.BookingCode,
					request.IsBookingForSomeoneElse ? " (for someone else)" : "");

				// Reload with details
				var savedBooking = await _bookingRepository.GetByIdWithDetailsAsync(booking.Id);
				return _mapper.Map<BookingDto>(savedBooking);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating booking.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		// BookingSystem.Application.Services/BookingService.cs
		public async Task<BookingDto?> UpdateBookingAsync(int bookingId, int userId, UpdateBookingDto request)
		{
			_logger.LogInformation("Updating booking {BookingId} by user {UserId}.", bookingId, userId);

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isHost = roles.Contains("Host") && booking.Homestay.OwnerId == userId;
			var isGuest = booking.GuestId == userId;

			// Only guest, host, or admin can update
			if (!isAdmin && !isHost && !isGuest)
			{
				throw new BadRequestException("You do not have permission to update this booking.");
			}

			// Can only update if status is Pending or Confirmed
			if (!isAdmin)
			{
				if (booking.BookingStatus != BookingStatus.Pending && booking.BookingStatus != BookingStatus.Confirmed)
				{
					throw new BadRequestException("Booking cannot be updated in its current status.");
				}
			}

			// ✅ THÊM: Validate thông tin người ở thực tế
			if (request.IsBookingForSomeoneElse.HasValue && request.IsBookingForSomeoneElse.Value)
			{
				// Nếu đổi sang đặt cho người khác, phải có thông tin người ở
				if (string.IsNullOrWhiteSpace(request.ActualGuestFullName) &&
					string.IsNullOrWhiteSpace(booking.ActualGuestFullName))
				{
					throw new BadRequestException("Actual guest full name is required when booking for someone else.");
				}

				if (string.IsNullOrWhiteSpace(request.ActualGuestPhoneNumber) &&
					string.IsNullOrWhiteSpace(booking.ActualGuestPhoneNumber))
				{
					throw new BadRequestException("Actual guest phone number is required when booking for someone else.");
				}
			}

			// Check if dates are being changed
			var datesChanged = false;
			if (request.CheckInDate.HasValue || request.CheckOutDate.HasValue)
			{
				var newCheckIn = request.CheckInDate ?? booking.CheckInDate;
				var newCheckOut = request.CheckOutDate ?? booking.CheckOutDate;

				if (newCheckIn != booking.CheckInDate || newCheckOut != booking.CheckOutDate)
				{
					datesChanged = true;

					// Validate new dates
					if (newCheckOut <= newCheckIn)
					{
						throw new BadRequestException("Check-out date must be after check-in date.");
					}

					if (newCheckIn.Date < DateTime.UtcNow.Date)
					{
						throw new BadRequestException("Check-in date cannot be in the past.");
					}

					await UnblockDatesForBookingAsync(booking);

					try
					{
						// ✅ 2. Check availability với ngày mới
						var isAvailable = await IsHomestayAvailableAsync(
							booking.HomestayId, newCheckIn, newCheckOut, bookingId);

						if (!isAvailable)
						{
							throw new BadRequestException("Homestay is not available for the new dates.");
						}
					}
					catch
					{
						// ✅ 3. Nếu fail, block lại ngày cũ (rollback)
						await BlockDatesForBookingAsync(booking,
							booking.BookingStatus == BookingStatus.Confirmed ? "Confirmed Booking" : "Pending Booking");
						throw; // Re-throw exception
					}
				}
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Update basic fields
				if (request.CheckInDate.HasValue)
					booking.CheckInDate = request.CheckInDate.Value;

				if (request.CheckOutDate.HasValue)
					booking.CheckOutDate = request.CheckOutDate.Value;

				if (request.NumberOfGuests.HasValue)
				{
					if (request.NumberOfGuests.Value > booking.Homestay.MaximumGuests)
					{
						throw new BadRequestException($"Maximum number of guests is {booking.Homestay.MaximumGuests}.");
					}
					booking.NumberOfGuests = request.NumberOfGuests.Value;
				}

				if (request.NumberOfAdults.HasValue)
					booking.NumberOfAdults = request.NumberOfAdults.Value;

				if (request.NumberOfChildren.HasValue)
					booking.NumberOfChildren = request.NumberOfChildren.Value;

				if (request.NumberOfInfants.HasValue)
					booking.NumberOfInfants = request.NumberOfInfants.Value;

				if (request.SpecialRequests != null)
					booking.SpecialRequests = request.SpecialRequests;

				// ✅ THÊM: Update thông tin người đặt (CHỈ ADMIN)
				if (isAdmin)
				{
					if (!string.IsNullOrWhiteSpace(request.GuestFullName))
						booking.GuestFullName = request.GuestFullName;

					if (!string.IsNullOrWhiteSpace(request.GuestEmail))
						booking.GuestEmail = request.GuestEmail;

					if (!string.IsNullOrWhiteSpace(request.GuestPhoneNumber))
						booking.GuestPhoneNumber = request.GuestPhoneNumber;

					if (request.GuestAddress != null)
						booking.GuestAddress = request.GuestAddress;

					if (request.GuestCity != null)
						booking.GuestCity = request.GuestCity;

					if (request.GuestCountry != null)
						booking.GuestCountry = request.GuestCountry;

					_logger.LogInformation("Admin {UserId} updated guest info for booking {BookingId}.", userId, bookingId);
				}

				// ✅ THÊM: Update thông tin người ở thực tế (GUEST hoặc ADMIN)
				if (isGuest || isAdmin)
				{
					if (request.IsBookingForSomeoneElse.HasValue)
					{
						booking.IsBookingForSomeoneElse = request.IsBookingForSomeoneElse.Value;

						// Nếu đổi sang KHÔNG đặt cho người khác, xóa thông tin ActualGuest
						if (!request.IsBookingForSomeoneElse.Value)
						{
							booking.ActualGuestFullName = null;
							booking.ActualGuestEmail = null;
							booking.ActualGuestPhoneNumber = null;
							booking.ActualGuestIdNumber = null;
							booking.ActualGuestNotes = null;

							_logger.LogInformation("Booking {BookingId} changed to self-booking. Cleared actual guest info.", bookingId);
						}
					}

					// Chỉ update nếu đang đặt cho người khác
					if (booking.IsBookingForSomeoneElse)
					{
						if (!string.IsNullOrWhiteSpace(request.ActualGuestFullName))
							booking.ActualGuestFullName = request.ActualGuestFullName;

						if (request.ActualGuestEmail != null)
							booking.ActualGuestEmail = request.ActualGuestEmail;

						if (!string.IsNullOrWhiteSpace(request.ActualGuestPhoneNumber))
							booking.ActualGuestPhoneNumber = request.ActualGuestPhoneNumber;

						if (request.ActualGuestIdNumber != null)
							booking.ActualGuestIdNumber = request.ActualGuestIdNumber;

						if (request.ActualGuestNotes != null)
							booking.ActualGuestNotes = request.ActualGuestNotes;

						_logger.LogInformation("Updated actual guest info for booking {BookingId}.", bookingId);
					}
				}
				else if (!isAdmin && (request.GuestFullName != null || request.GuestEmail != null || request.GuestPhoneNumber != null))
				{
					// Host cố gắng update thông tin người đặt
					throw new BadRequestException("Only admin can update guest information.");
				}

				// Recalculate price if dates changed
				// Recalculate price if dates changed
				if (datesChanged)
				{
					var priceBreakdown = await CalculateBookingPriceAsync(new BookingPriceCalculationDto
					{
						HomestayId = booking.HomestayId,
						CheckInDate = booking.CheckInDate,
						CheckOutDate = booking.CheckOutDate,
						NumberOfGuests = booking.NumberOfGuests
					});

					var oldTotalAmount = booking.TotalAmount;
					var oldBaseAmount = booking.BaseAmount;

					booking.BaseAmount = priceBreakdown.BaseAmount;
					booking.CleaningFee = priceBreakdown.CleaningFee;
					booking.ServiceFee = priceBreakdown.ServiceFee;
					booking.TaxAmount = priceBreakdown.TaxAmount;

					// ✅ XỬ LÝ MÃ GIẢM GIÁ: Tính lại discount dựa trên coupon đã dùng
					var couponUsages = await _couponUsageRepository.GetCouponUsagesWithCouponByBookingIdAsync(booking.Id);

					decimal totalCouponDiscount = 0;

					if (couponUsages.Any())
					{
						foreach (var usage in couponUsages)
						{
							var coupon = usage.Coupon;

							// Kiểm tra coupon còn hiệu lực
							if (coupon.IsActive && coupon.EndDate >= DateTime.UtcNow) // Sửa: EndDate (không có ExpiryDate)
							{
								decimal discount = 0;

								if (coupon.CouponType == CouponType.Percentage) // Sửa: CouponType (không phải DiscountType)
								{
									// Tính % discount dựa trên BaseAmount mới
									discount = booking.BaseAmount * (coupon.DiscountValue / 100);

									// Giới hạn theo MaxDiscountAmount nếu có
									if (coupon.MaxDiscountAmount.HasValue)
									{
										discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);
									}
								}
								else // FixedAmount
								{
									discount = coupon.DiscountValue;
								}

								// Cập nhật DiscountAmount mới cho CouponUsage
								usage.DiscountAmount = discount;
								totalCouponDiscount += discount;

								_logger.LogInformation(
									"Recalculated discount for coupon {CouponCode}: {Discount}",
									coupon.CouponCode, discount); // Sửa: CouponCode (không phải Code)
							}
							else
							{
								_logger.LogWarning(
									"Coupon {CouponCode} is no longer valid. Removing discount.",
									coupon.CouponCode);
							}
						}

						// Lưu lại DiscountAmount mới vào CouponUsage
						_couponUsageRepository.UpdateCouponUsagesRange(couponUsages); // Dùng UpdateRange thay vì Update
					}

					// DiscountAmount từ hệ thống (weekly/monthly) + coupon
					var systemDiscount = priceBreakdown.DiscountAmount; // Weekly/Monthly discount
					booking.DiscountAmount = systemDiscount + totalCouponDiscount;

					// Tính lại TotalAmount với discount mới
					var subtotal = booking.BaseAmount - systemDiscount + booking.CleaningFee + booking.ServiceFee;
					booking.TotalAmount = subtotal + booking.TaxAmount - totalCouponDiscount;

					// Đảm bảo TotalAmount không âm
					if (booking.TotalAmount < 0) booking.TotalAmount = 0;

					// ✅ XỬ LÝ PAYMENT
					var totalPaid = booking.Payments
						.Where(p => p.PaymentStatus == PaymentStatus.Completed)
						.Sum(p => p.PaymentAmount);

					var priceDifference = booking.TotalAmount - oldTotalAmount;

					if (priceDifference > 0)
					{
						var remainingAmount = booking.TotalAmount - totalPaid;

						if (remainingAmount > 0)
						{
							_logger.LogInformation(
								"Booking {BookingId} price increased by {Difference}. Customer needs to pay additional {Remaining}.",
								bookingId, priceDifference, remainingAmount);

							if (booking.BookingStatus == BookingStatus.Confirmed && !isAdmin)
							{
								booking.BookingStatus = BookingStatus.Pending;
								booking.PaymentNotes = $"Cần thanh toán thêm {remainingAmount:N0} VNĐ do thay đổi đặt phòng";

								_logger.LogInformation(
									"Booking {BookingId} status changed to Pending due to price increase.",
									bookingId);
							}
						}
					}
					else if (priceDifference < 0)
					{
						var overpaidAmount = totalPaid - booking.TotalAmount;

						if (overpaidAmount > 0)
						{
							_logger.LogInformation(
								"Booking {BookingId} price decreased by {Difference}. Overpaid amount: {Overpaid}.",
								bookingId, Math.Abs(priceDifference), overpaidAmount);

							booking.PaymentNotes = $"Đã thanh toán thừa {overpaidAmount:N0} VNĐ, sẽ được hoàn lại";
						}
					}

					_logger.LogInformation(
						"Recalculated price for booking {BookingId}. Old: {OldTotal}, New: {NewTotal}, Paid: {TotalPaid}, CouponDiscount: {CouponDiscount}",
						bookingId, oldTotalAmount, booking.TotalAmount, totalPaid, totalCouponDiscount);
				}

				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingId} updated successfully.", bookingId);

				return _mapper.Map<BookingDto>(booking);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating booking {BookingId}.", bookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<BookingDto?> GetByIdAsync(int bookingId, int userId)
		{
			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isHost = roles.Contains("Host") && booking.Homestay.OwnerId == userId;
			var isGuest = booking.GuestId == userId;

			// Only guest, host, or admin can view
			if (!isAdmin && !isHost && !isGuest)
			{
				throw new BadRequestException("You do not have permission to view this booking.");
			}

			return _mapper.Map<BookingDto>(booking);
		}

		public async Task<BookingDto?> GetByBookingCodeAsync(string bookingCode)
		{
			var booking = await _bookingRepository.GetByBookingCodeAsync(bookingCode);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with code {bookingCode} not found.");
			}

			return _mapper.Map<BookingDto>(booking);
		}

		public async Task<PagedResult<BookingDto>> GetUserBookingsAsync(int userId, BookingFilter filter)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var pagedBookings = await _bookingRepository.GetUserBookingsAsync(userId, filter);
			var bookingDtos = _mapper.Map<List<BookingDto>>(pagedBookings.Items);

			return new PagedResult<BookingDto>
			{
				Items = bookingDtos,
				TotalCount = pagedBookings.TotalCount,
				PageSize = pagedBookings.PageSize,
				PageNumber = pagedBookings.PageNumber
			};
		}

		public async Task<PagedResult<BookingDto>> GetHostBookingsAsync(int hostId, BookingFilter filter)
		{
			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				throw new BadRequestException("User does not have Host or Admin role.");
			}

			var pagedBookings = await _bookingRepository.GetHostBookingsAsync(hostId, filter);
			var bookingDtos = _mapper.Map<List<BookingDto>>(pagedBookings.Items);

			return new PagedResult<BookingDto>
			{
				Items = bookingDtos,
				TotalCount = pagedBookings.TotalCount,
				PageSize = pagedBookings.PageSize,
				PageNumber = pagedBookings.PageNumber
			};
		}

		public async Task<PagedResult<BookingDto>> GetAllBookingsAsync(BookingFilter filter)
		{
			var pagedBookings = await _bookingRepository.GetAllBookingsAsync(filter);
			var bookingDtos = _mapper.Map<List<BookingDto>>(pagedBookings.Items);

			return new PagedResult<BookingDto>
			{
				Items = bookingDtos,
				TotalCount = pagedBookings.TotalCount,
				PageSize = pagedBookings.PageSize,
				PageNumber = pagedBookings.PageNumber
			};
		}

		public async Task<bool> ConfirmBookingAsync(int bookingId, int hostId)
		{
			_logger.LogInformation("Confirming booking {BookingId} by host {HostId}.", bookingId, hostId);

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			var isAdmin = roles.Contains("Admin");

			// Chỉ cho phép Admin hoặc chủ Homestay xác nhận
			if (!isAdmin && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to confirm this booking.");
			}

			// Nếu không phải Admin thì chỉ được xác nhận khi trạng thái là Pending
			if (!isAdmin && booking.BookingStatus != BookingStatus.Pending)
			{
				throw new BadRequestException("Only pending bookings can be confirmed.");
			}

			// Nếu không phải Admin thì phải kiểm tra xem homestay còn trống không
			if (!isAdmin)
			{
				var isAvailable = await IsHomestayAvailableAsync(
					booking.HomestayId, booking.CheckInDate, booking.CheckOutDate, bookingId);

				if (!isAvailable)
				{
					throw new BadRequestException("Homestay is no longer available for these dates.");
				}
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				booking.BookingStatus = BookingStatus.Confirmed;
				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				// TODO: Send confirmation email/notification to guest
				await BlockDatesForBookingAsync(booking, "Confirmed Booking");
				await _unitOfWork.CommitTransactionAsync();

				_ = _emailService.SendBookingConfirmationAsync(
					booking.Guest.Email!,
					booking.Guest.FullName,
					booking.BookingCode,
					booking.Homestay.HomestayTitle,
					booking.CheckInDate,
					booking.CheckOutDate,
					booking.TotalAmount
				);
				_logger.LogInformation("Booking {BookingId} confirmed successfully by {Role} {UserId}.",
					bookingId, isAdmin ? "Admin" : "Host", hostId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while confirming booking {BookingId}.", bookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}


		public async Task<bool> RejectBookingAsync(int bookingId, int hostId, string reason)
		{
			_logger.LogInformation("Rejecting booking {BookingId} by host {HostId}.", bookingId, hostId);

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			var isAdmin = roles.Contains("Admin");

			// Chỉ cho phép Admin hoặc chủ Homestay từ chối
			if (!isAdmin && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to reject this booking.");
			}

			// Nếu không phải Admin, chỉ có thể từ chối khi booking đang ở trạng thái Pending
			if (!isAdmin && booking.BookingStatus != BookingStatus.Pending)
			{
				throw new BadRequestException("Only pending bookings can be rejected.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				booking.BookingStatus = BookingStatus.Rejected;
				booking.CancellationReason = reason;
				booking.CancelledAt = DateTime.UtcNow;
				booking.CancelledBy = hostId.ToString();
				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				// TODO: Send rejection email/notification to guest
				// TODO: Process refund if payment was made
				await UnblockDatesForBookingAsync(booking);

				var homestayToRestore = await _homestayRepository.GetByIdAsync(booking.HomestayId);
				if (homestayToRestore != null)
				{
					homestayToRestore.RoomsAtThisPrice += 1;
					_homestayRepository.Update(homestayToRestore);
					_logger.LogInformation("RoomsAtThisPrice restored for homestay {HomestayId} after cancellation.", booking.HomestayId);
				}
				await _unitOfWork.CommitTransactionAsync();

				_ = _emailService.SendBookingRejectedAsync(
					booking.Guest.Email!,
					booking.Guest.FullName,
					booking.BookingCode,
					booking.Homestay.HomestayTitle,
					reason
				);
				_logger.LogInformation("Booking {BookingId} rejected successfully by {Role} {UserId}.",
					bookingId, isAdmin ? "Admin" : "Host", hostId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while rejecting booking {BookingId}.", bookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}



		public async Task<bool> CancelBookingAsync(int bookingId, int userId, CancelBookingDto request)
		{
			_logger.LogInformation("Cancelling booking {BookingId} by user {UserId}.", bookingId, userId);

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isGuest = booking.GuestId == userId;

			// Chỉ cho phép Admin hoặc chính Guest hủy
			if (!isAdmin && !isGuest)
			{
				throw new BadRequestException("You do not have permission to cancel this booking.");
			}

			// Nếu không phải Admin thì chỉ được hủy khi trạng thái là Pending hoặc Confirmed
			if (!isAdmin && booking.BookingStatus != BookingStatus.Pending && booking.BookingStatus != BookingStatus.Confirmed)
			{
				throw new BadRequestException("Only pending or confirmed bookings can be cancelled.");
			}

			// Nếu không phải Admin thì không được hủy khi đã check-in
			if (!isAdmin && booking.BookingStatus == BookingStatus.CheckedIn)
			{
				throw new BadRequestException("Cannot cancel a booking that is already checked in.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				booking.BookingStatus = BookingStatus.Cancelled;
				booking.CancellationReason = request.CancellationReason;
				booking.CancelledAt = DateTime.UtcNow;
				booking.CancelledBy = userId.ToString();
				booking.UpdatedAt = DateTime.UtcNow;

				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				// TODO: Calculate cancellation fee based on cancellation policy
				// TODO: Process refund
				await UnblockDatesForBookingAsync(booking);

				var homestayToRestore = await _homestayRepository.GetByIdAsync(booking.HomestayId);
				if (homestayToRestore != null)
				{
					homestayToRestore.RoomsAtThisPrice += 1;
					_homestayRepository.Update(homestayToRestore);
					_logger.LogInformation("RoomsAtThisPrice restored for homestay {HomestayId} after cancellation.", booking.HomestayId);
				}
				await _unitOfWork.CommitTransactionAsync();
				_ = _emailService.SendBookingCancelledAsync(
					booking.Guest.Email!,
					booking.Guest.FullName,
					booking.BookingCode,
					booking.Homestay.HomestayTitle
				);
				_logger.LogInformation("Booking {BookingId} cancelled successfully by {Role} {UserId}.",
					bookingId, isAdmin ? "Admin" : "Guest", userId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while cancelling booking {BookingId}.", bookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}


		public async Task<bool> CheckInAsync(int bookingId, int hostId)
		{
			_logger.LogInformation("Checking in booking {BookingId} by host {HostId}.", bookingId, hostId);

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			var isAdmin = roles.Contains("Admin");

			// Chỉ cho phép Host của homestay hoặc Admin thực hiện
			if (!isAdmin && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to check in this booking.");
			}

			// Nếu không phải Admin thì chỉ được check-in khi booking ở trạng thái Confirmed
			if (!isAdmin && booking.BookingStatus != BookingStatus.Confirmed)
			{
				throw new BadRequestException("Only confirmed bookings can be checked in.");
			}

			// Nếu không phải Admin thì không được check-in trước ngày CheckIn
			if (!isAdmin && booking.CheckInDate.Date > DateTime.UtcNow.Date)
			{
				throw new BadRequestException("Cannot check in before the check-in date.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				booking.BookingStatus = BookingStatus.CheckedIn;
				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingId} checked in successfully by {Role} {UserId}.",
					bookingId, isAdmin ? "Admin" : "Host", hostId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while checking in booking {BookingId}.", bookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}


		public async Task<bool> CheckOutAsync(int bookingId, int hostId)
		{
			_logger.LogInformation("Checking out booking {BookingId} by host {HostId}.", bookingId, hostId);

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			bool isAdmin = roles.Contains("Admin");

			// ✅ Admin có toàn quyền, các role khác phải là chủ homestay
			if (!isAdmin && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to check out this booking.");
			}

			// ✅ Admin có thể bỏ qua giới hạn trạng thái nếu cần
			if (!isAdmin && booking.BookingStatus != BookingStatus.CheckedIn)
			{
				throw new BadRequestException("Only checked-in bookings can be checked out.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();
				_logger.LogInformation("Transaction started for booking {BookingId}.", bookingId);

				booking.BookingStatus = BookingStatus.CheckedOut;
				booking.UpdatedAt = DateTime.UtcNow;

				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingId} checked out successfully by host {HostId}.", bookingId, hostId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while checking out booking {BookingId}.", bookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}


		public async Task<bool> MarkAsCompletedAsync(int bookingId, int userId)
		{
			_logger.LogInformation("Marking booking {BookingId} as completed by user {UserId}.", bookingId, userId);

			var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			bool isAdmin = roles.Contains("Admin");

			// ✅ Admin có toàn quyền, host thì phải là chủ homestay
			if (!isAdmin && booking.Homestay.OwnerId != userId)
			{
				throw new BadRequestException("You do not have permission to complete this booking.");
			}

			// ✅ Admin có thể bỏ qua trạng thái, host thì không
			if (!isAdmin && booking.BookingStatus != BookingStatus.CheckedOut)
			{
				throw new BadRequestException("Only checked-out bookings can be marked as completed.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				booking.BookingStatus = BookingStatus.Completed;
				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingId} marked as completed successfully by user {UserId}.", bookingId, userId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while marking booking {BookingId} as completed.", bookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}


		public async Task<bool> MarkAsNoShowAsync(int bookingId, int hostId)
{
	_logger.LogInformation("Marking booking {BookingId} as no-show by host {HostId}.", bookingId, hostId);

	var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
	if (booking == null)
	{
		throw new NotFoundException($"Booking with ID {bookingId} not found.");
	}

	var host = await _userManager.FindByIdAsync(hostId.ToString());
	if (host == null)
	{
		throw new NotFoundException($"Host with ID {hostId} not found.");
	}

	var roles = await _userManager.GetRolesAsync(host);
	bool isAdmin = roles.Contains("Admin");

	// ✅ Admin có toàn quyền
	if (!isAdmin && booking.Homestay.OwnerId != hostId)
	{
		throw new BadRequestException("You do not have permission to mark this booking as no-show.");
	}

	// ✅ Admin có thể bỏ qua điều kiện trạng thái
	if (!isAdmin && booking.BookingStatus != BookingStatus.Confirmed)
	{
		throw new BadRequestException("Only confirmed bookings can be marked as no-show.");
	}

	// ✅ Admin có thể bỏ qua điều kiện ngày
	if (!isAdmin && booking.CheckInDate.Date >= DateTime.UtcNow.Date)
	{
		throw new BadRequestException("Cannot mark as no-show before the check-in date has passed.");
	}

	try
	{
		await _unitOfWork.BeginTransactionAsync();

		booking.BookingStatus = BookingStatus.NoShow;
		booking.UpdatedAt = DateTime.UtcNow;
		_bookingRepository.Update(booking);
		await _bookingRepository.SaveChangesAsync();

		// TODO: Apply no-show penalty/fee

		await _unitOfWork.CommitTransactionAsync();
		_logger.LogInformation("Booking {BookingId} marked as no-show successfully by host {HostId}.", bookingId, hostId);

		return true;
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error occurred while marking booking {BookingId} as no-show.", bookingId);
		await _unitOfWork.RollbackTransactionAsync();
		throw;
	}
}

		public async Task<bool> IsHomestayAvailableAsync(
			int homestayId,
			DateTime checkInDate,
			DateTime checkOutDate,
			int? excludeBookingId = null)
		{
			// Check homestay exists and is active
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null || !homestay.IsActive || !homestay.IsApproved)
			{
				return false;
			}

			// Check for overlapping bookings
			var hasOverlappingBookings = await _bookingRepository.HasOverlappingBookingsAsync(
				homestayId, checkInDate, checkOutDate, excludeBookingId);

			if (hasOverlappingBookings)
			{
				return false;
			}

			// Check availability calendar
			var startDate = DateOnly.FromDateTime(checkInDate);
			var endDate = DateOnly.FromDateTime(checkOutDate).AddDays(-1); // Don't include checkout date

			// ✅ THÊM: Lấy booking code để loại trừ
			string excludeBookingCode = null;
			if (excludeBookingId.HasValue)
			{
				var booking = await _bookingRepository.GetByIdAsync(excludeBookingId.Value);
				excludeBookingCode = booking?.BookingCode;
			}

			var isCalendarAvailable = await _availabilityCalendarRepository.IsDateRangeAvailableAsync(
				homestayId, startDate, endDate, excludeBookingCode);

			return isCalendarAvailable;
		}

		public async Task<BookingStatisticsDto> GetBookingStatisticsAsync(
			int? homestayId = null,
			int? hostId = null,
			DateTime? startDate = null,
			DateTime? endDate = null)
		{
			_logger.LogInformation("Getting booking statistics.");

			var filter = new BookingFilter
			{
				HomestayId = homestayId,
				HostId = hostId,
				CheckInDateFrom = startDate,
				CheckInDateTo = endDate,
				PageNumber = 1,
				PageSize = int.MaxValue // Get all records for statistics
			};

			IEnumerable<Booking> bookings;

			if (hostId.HasValue)
			{
				var pagedResult = await _bookingRepository.GetHostBookingsAsync(hostId.Value, filter);
				bookings = pagedResult.Items;
			}
			else if (homestayId.HasValue)
			{
				var pagedResult = await _bookingRepository.GetHomestayBookingsAsync(homestayId.Value, filter);
				bookings = pagedResult.Items;
			}
			else
			{
				var pagedResult = await _bookingRepository.GetAllBookingsAsync(filter);
				bookings = pagedResult.Items;
			}

			var totalBookings = bookings.Count();
			var pendingBookings = bookings.Count(b => b.BookingStatus == BookingStatus.Pending);
			var confirmedBookings = bookings.Count(b => b.BookingStatus == BookingStatus.Confirmed);
			var completedBookings = bookings.Count(b => b.BookingStatus == BookingStatus.Completed || b.BookingStatus == BookingStatus.CheckedOut);
			var cancelledBookings = bookings.Count(b => b.BookingStatus == BookingStatus.Cancelled);

			var completedBookingsList = bookings.Where(b =>
				b.BookingStatus == BookingStatus.Completed || b.BookingStatus == BookingStatus.CheckedOut).ToList();

			var totalRevenue = completedBookingsList.Sum(b => b.TotalAmount);
			var averageBookingValue = completedBookings > 0 ? totalRevenue / completedBookings : 0;

			// Calculate occupancy rate
			double occupancyRate = 0;
			if (homestayId.HasValue && startDate.HasValue && endDate.HasValue)
			{
				var totalDays = (endDate.Value - startDate.Value).Days;
				var bookedDays = completedBookingsList.Sum(b => (b.CheckOutDate - b.CheckInDate).Days);
				occupancyRate = totalDays > 0 ? (double)bookedDays / totalDays * 100 : 0;
			}

			return new BookingStatisticsDto
			{
				TotalBookings = totalBookings,
				PendingBookings = pendingBookings,
				ConfirmedBookings = confirmedBookings,
				CompletedBookings = completedBookings,
				CancelledBookings = cancelledBookings,
				TotalRevenue = totalRevenue,
				AverageBookingValue = averageBookingValue,
				OccupancyRate = Math.Round(occupancyRate, 2)
			};
		}

		public async Task ProcessExpiredPendingBookingsAsync()
		{
			_logger.LogInformation("Processing expired unpaid bookings.");

			var expiredBookings = await _bookingRepository.GetUnpaidExpiredBookingsAsync();

			if (!expiredBookings.Any())
			{
				_logger.LogInformation("No expired unpaid bookings found.");
				return;
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				foreach (var booking in expiredBookings)
				{
					booking.BookingStatus = BookingStatus.Cancelled;
					booking.CancellationReason = "Booking automatically cancelled due to payment timeout (30 minutes). No payment received.";
					booking.CancelledAt = DateTime.UtcNow;
					booking.CancelledBy = "System";
					booking.UpdatedAt = DateTime.UtcNow;

					_bookingRepository.Update(booking);

					// Unblock các ngày đã được block
					await UnblockDatesForBookingAsync(booking);
					var homestayToRestore = await _homestayRepository.GetByIdAsync(booking.HomestayId);
					if (homestayToRestore != null)
					{
						homestayToRestore.RoomsAtThisPrice += 1;
						_homestayRepository.Update(homestayToRestore);
						_logger.LogInformation("RoomsAtThisPrice restored for homestay {HomestayId} after cancellation.", booking.HomestayId);
					}
					_logger.LogInformation("Booking {BookingCode} auto-cancelled due to payment timeout.", booking.BookingCode);

					// GỬI EMAIL THÔNG BÁO CHO GUEST
					_ = _emailService.SendBookingCancelledAsync(
						booking.Guest.Email!,
						booking.Guest.FullName,
						booking.BookingCode,
						booking.Homestay.HomestayTitle
					);
				}

				await _bookingRepository.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Successfully processed {Count} expired unpaid bookings.", expiredBookings.Count());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while processing expired unpaid bookings.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		private async Task UnblockDatesForBookingAsync(Booking booking)
		{
			_logger.LogInformation(
				"Unblocking dates for cancelled Booking {BookingCode}.",
				booking.BookingCode
			);

			var startDate = DateOnly.FromDateTime(booking.CheckInDate);
			var endDate = DateOnly.FromDateTime(booking.CheckOutDate).AddDays(-1);

			var calendars = await _availabilityCalendarRepository
				.GetByDateRangeAsync(booking.HomestayId, startDate, endDate);

			foreach (var calendar in calendars)
			{
				// Chỉ unblock nếu BlockReason chứa BookingCode này
				if (calendar.BlockReason != null &&
					calendar.BlockReason.Contains(booking.BookingCode))
				{
					calendar.IsBlocked = false;
					calendar.IsAvailable = true;
					calendar.BlockReason = null;
					calendar.UpdatedAt = DateTime.UtcNow;
					calendar.UpdatedBy = "System";

					_availabilityCalendarRepository.Update(calendar);
				}
			}

			await _availabilityCalendarRepository.SaveChangesAsync();

			_logger.LogInformation(
				"Unblocked dates for cancelled Booking {BookingCode}.",
				booking.BookingCode
			);
		}

		private async Task BlockDatesForBookingAsync(Booking booking, string reason = "Booked")
		{
			_logger.LogInformation(
				"Blocking dates for Booking {BookingCode} from {CheckIn} to {CheckOut}.",
				booking.BookingCode, booking.CheckInDate, booking.CheckOutDate
			);

			var startDate = DateOnly.FromDateTime(booking.CheckInDate);
			var endDate = DateOnly.FromDateTime(booking.CheckOutDate).AddDays(-1); // Không bao gồm ngày checkout

			var datesToBlock = new List<DateOnly>();
			var currentDate = startDate;

			// Tạo danh sách các ngày cần block
			while (currentDate <= endDate)
			{
				datesToBlock.Add(currentDate);
				currentDate = currentDate.AddDays(1);
			}

			// Lấy các calendar entries đã tồn tại
			var existingCalendars = await _availabilityCalendarRepository
				.GetByDateRangeAsync(booking.HomestayId, startDate, endDate);

			var existingDates = existingCalendars
				.ToDictionary(c => c.AvailableDate, c => c);

			foreach (var date in datesToBlock)
			{
				if (existingDates.ContainsKey(date))
				{
					// Update existing calendar entry
					var calendar = existingDates[date];
					calendar.IsBlocked = true;
					calendar.IsAvailable = false;
					calendar.BlockReason = $"{reason} - {booking.BookingCode}";
					calendar.UpdatedAt = DateTime.UtcNow;
					calendar.UpdatedBy = "System";

					_availabilityCalendarRepository.Update(calendar);
				}
				else
				{
					// Create new calendar entry
					var newCalendar = new AvailabilityCalendar
					{
						HomestayId = booking.HomestayId,
						AvailableDate = date,
						IsBlocked = true,
						IsAvailable = false,
						BlockReason = $"{reason} - {booking.BookingCode}",
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow,
						CreatedBy = "System",
						UpdatedBy = "System"
					};

					await _availabilityCalendarRepository.AddAsync(newCalendar);
				}
			}

			await _availabilityCalendarRepository.SaveChangesAsync();

			_logger.LogInformation(
				"Blocked {Count} dates for Booking {BookingCode}.",
				datesToBlock.Count, booking.BookingCode
			);
		}

		public async Task<PagedResult<BookingDto>> GetHostManagedBookingsPagedAsync(int hostId, BookingFilter filter)
		{
			_logger.LogInformation("Fetching paged bookings for host {HostId}.", hostId);

			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
				throw new NotFoundException($"Host with ID {hostId} not found.");

			var roles = await _userManager.GetRolesAsync(host);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
				throw new BadRequestException("User does not have Host or Admin role.");

			var pagedBookings = await _bookingRepository.GetHostBookingsAsync(hostId, filter);
			var dtos = _mapper.Map<List<BookingDto>>(pagedBookings.Items);

			return new PagedResult<BookingDto>
			{
				Items = dtos,
				TotalCount = pagedBookings.TotalCount,
				PageNumber = pagedBookings.PageNumber,
				PageSize = pagedBookings.PageSize
			};
		}
	}
}
