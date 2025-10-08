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
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services
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
			ILogger<BookingService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_bookingRepository = bookingRepository;
			_homestayRepository = homestayRepository;
			_availabilityCalendarRepository = availabilityCalendarRepository;
			_userManager = userManager;
			_logger = logger;
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
					isWeekend = dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday;

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
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _bookingRepository.AddAsync(booking);
				await _bookingRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingCode} created successfully.", booking.BookingCode);

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
			if (booking.BookingStatus != BookingStatus.Pending && booking.BookingStatus != BookingStatus.Confirmed)
			{
				throw new BadRequestException("Booking cannot be updated in its current status.");
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

					// Check availability for new dates (excluding current booking)
					var isAvailable = await IsHomestayAvailableAsync(
						booking.HomestayId, newCheckIn, newCheckOut, bookingId);

					if (!isAvailable)
					{
						throw new BadRequestException("Homestay is not available for the new dates.");
					}
				}
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Update fields
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

					booking.BaseAmount = priceBreakdown.BaseAmount;
					booking.CleaningFee = priceBreakdown.CleaningFee;
					booking.ServiceFee = priceBreakdown.ServiceFee;
					booking.TaxAmount = priceBreakdown.TaxAmount;
					booking.DiscountAmount = priceBreakdown.DiscountAmount;
					booking.TotalAmount = priceBreakdown.TotalAmount;
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
			if (!roles.Contains("Admin") && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to confirm this booking.");
			}

			if (booking.BookingStatus != BookingStatus.Pending)
			{
				throw new BadRequestException("Only pending bookings can be confirmed.");
			}

			// Check if still available
			var isAvailable = await IsHomestayAvailableAsync(
				booking.HomestayId, booking.CheckInDate, booking.CheckOutDate, bookingId);

			if (!isAvailable)
			{
				throw new BadRequestException("Homestay is no longer available for these dates.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				booking.BookingStatus = BookingStatus.Confirmed;
				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				// TODO: Send confirmation email/notification to guest

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingId} confirmed successfully.", bookingId);

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
			if (!roles.Contains("Admin") && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to reject this booking.");
			}

			if (booking.BookingStatus != BookingStatus.Pending)
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

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingId} rejected successfully.", bookingId);

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

			if (!isAdmin && !isGuest)
			{
				throw new BadRequestException("You do not have permission to cancel this booking.");
			}

			if (booking.BookingStatus != BookingStatus.Pending && booking.BookingStatus != BookingStatus.Confirmed)
			{
				throw new BadRequestException("Only pending or confirmed bookings can be cancelled.");
			}

			// Check if already checked in
			if (booking.BookingStatus == BookingStatus.CheckedIn)
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

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingId} cancelled successfully.", bookingId);

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
			if (!roles.Contains("Admin") && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to check in this booking.");
			}

			if (booking.BookingStatus != BookingStatus.Confirmed)
			{
				throw new BadRequestException("Only confirmed bookings can be checked in.");
			}

			// Check if check-in date is today or in the past
			if (booking.CheckInDate.Date > DateTime.UtcNow.Date)
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
				_logger.LogInformation("Booking {BookingId} checked in successfully.", bookingId);

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
			if (!roles.Contains("Admin") && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to check out this booking.");
			}

			if (booking.BookingStatus != BookingStatus.CheckedIn)
			{
				throw new BadRequestException("Only checked-in bookings can be checked out.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				booking.BookingStatus = BookingStatus.CheckedOut;
				booking.UpdatedAt = DateTime.UtcNow;
				_bookingRepository.Update(booking);
				await _bookingRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Booking {BookingId} checked out successfully.", bookingId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while checking out booking {BookingId}.", bookingId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> MarkAsCompletedAsync(int bookingId)
		{
			_logger.LogInformation("Marking booking {BookingId} as completed.", bookingId);

			var booking = await _bookingRepository.GetByIdAsync(bookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {bookingId} not found.");
			}

			if (booking.BookingStatus != BookingStatus.CheckedOut)
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
				_logger.LogInformation("Booking {BookingId} marked as completed successfully.", bookingId);

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
			if (!roles.Contains("Admin") && booking.Homestay.OwnerId != hostId)
			{
				throw new BadRequestException("You do not have permission to mark this booking as no-show.");
			}

			if (booking.BookingStatus != BookingStatus.Confirmed)
			{
				throw new BadRequestException("Only confirmed bookings can be marked as no-show.");
			}

			// Check if check-in date has passed
			if (booking.CheckInDate.Date >= DateTime.UtcNow.Date)
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
				_logger.LogInformation("Booking {BookingId} marked as no-show successfully.", bookingId);

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

			var isCalendarAvailable = await _availabilityCalendarRepository.IsDateRangeAvailableAsync(
				homestayId, startDate, endDate);

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
			_logger.LogInformation("Processing expired pending bookings.");

			var expiredBookings = await _bookingRepository.GetExpiredPendingBookingsAsync(30); // 30 minutes expiration

			if (!expiredBookings.Any())
			{
				_logger.LogInformation("No expired pending bookings found.");
				return;
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				foreach (var booking in expiredBookings)
				{
					booking.BookingStatus = BookingStatus.Cancelled;
					booking.CancellationReason = "Booking expired due to pending payment timeout.";
					booking.CancelledAt = DateTime.UtcNow;
					booking.CancelledBy = "System";
					booking.UpdatedAt = DateTime.UtcNow;
					_bookingRepository.Update(booking);
				}

				await _bookingRepository.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Successfully processed {Count} expired pending bookings.", expiredBookings.Count());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while processing expired pending bookings.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}
	}
}
