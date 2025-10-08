using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services
{
	public class AvailabilityCalendarService : IAvailabilityCalendarService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IAvailabilityCalendarRepository _availabilityCalendarRepository;
		private readonly IHomestayRepository _homestayRepository;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<AvailabilityCalendarService> _logger;

		public AvailabilityCalendarService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IAvailabilityCalendarRepository availabilityCalendarRepository,
			IHomestayRepository homestayRepository,
			UserManager<User> userManager,
			ILogger<AvailabilityCalendarService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_availabilityCalendarRepository = availabilityCalendarRepository;
			_homestayRepository = homestayRepository;
			_userManager = userManager;
			_logger = logger;
		}

		public async Task<bool> DeleteByDateRangeAsync(int ownerId, int homestayId, DateOnly startDate, DateOnly endDate)
		{
			_logger.LogInformation("Deleting availability calendar entries for homestay {HomestayId} from {StartDate} to {EndDate}.",
				homestayId, startDate, endDate);

			await ValidateOwnerPermissionAsync(ownerId, homestayId);

			if (endDate < startDate)
			{
				throw new BadRequestException("End date must be after start date.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				await _availabilityCalendarRepository.DeleteByDateRangeAsync(homestayId, startDate, endDate);
				await _availabilityCalendarRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Successfully deleted availability calendar entries for homestay {HomestayId}.", homestayId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting availability calendar entries by date range.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		private async Task ValidateOwnerPermissionAsync(int ownerId, int homestayId)
		{
			var owner = await _userManager.FindByIdAsync(ownerId.ToString());
			if (owner == null)
			{
				_logger.LogWarning("Owner with ID {OwnerId} not found.", ownerId);
				throw new NotFoundException($"Owner with ID {ownerId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(owner);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				_logger.LogWarning("User with ID {OwnerId} does not have the 'Host' or 'Admin' role.", ownerId);
				throw new BadRequestException($"User with ID {ownerId} does not have the 'Host' or 'Admin' role.");
			}

			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			if (!roles.Contains("Admin") && homestay.OwnerId != owner.Id)
			{
				_logger.LogWarning("User with ID {OwnerId} does not have permission to manage this homestay's calendar.", ownerId);
				throw new BadRequestException($"User with ID {ownerId} does not have permission to manage this homestay's calendar.");
			}
		}

		public async Task<AvailabilityCalendarDto> CreateAsync(int ownerId, CreateAvailabilityCalendarDto request)
		{
			_logger.LogInformation("Creating availability calendar entry for homestay {HomestayId} on date {Date}.",
				request.HomestayId, request.AvailableDate);

			await ValidateOwnerPermissionAsync(ownerId, request.HomestayId);

			// Check if date is in the past
			if (request.AvailableDate < DateOnly.FromDateTime(DateTime.UtcNow))
			{
				_logger.LogWarning("Cannot create availability for past date {Date}.", request.AvailableDate);
				throw new BadRequestException("Cannot create availability for past dates.");
			}

			// Check if entry already exists
			var exists = await _availabilityCalendarRepository.ExistsAsync(request.HomestayId, request.AvailableDate);
			if (exists)
			{
				_logger.LogWarning("Availability calendar entry already exists for homestay {HomestayId} on date {Date}.",
					request.HomestayId, request.AvailableDate);
				throw new BadRequestException($"Availability calendar entry already exists for this date.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var calendar = _mapper.Map<AvailabilityCalendar>(request);
				calendar.CreatedAt = DateTime.UtcNow;
				calendar.UpdatedAt = DateTime.UtcNow;

				await _availabilityCalendarRepository.AddAsync(calendar);
				await _availabilityCalendarRepository.SaveChangesAsync();

				// Reload with includes
				var savedCalendar = await _availabilityCalendarRepository.GetByHomestayAndDateAsync(
					request.HomestayId, request.AvailableDate);

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Availability calendar entry created successfully with ID {CalendarId}.", calendar.Id);

				return _mapper.Map<AvailabilityCalendarDto>(savedCalendar);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating availability calendar entry.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<List<AvailabilityCalendarDto>> BulkCreateAsync(int ownerId, BulkCreateAvailabilityCalendarDto request)
		{
			_logger.LogInformation("Bulk creating availability calendar entries for homestay {HomestayId} from {StartDate} to {EndDate}.",
				request.HomestayId, request.StartDate, request.EndDate);

			await ValidateOwnerPermissionAsync(ownerId, request.HomestayId);

			if (request.EndDate < request.StartDate)
			{
				throw new BadRequestException("End date must be after start date.");
			}

			if (request.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
			{
				throw new BadRequestException("Cannot create availability for past dates.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var calendars = new List<AvailabilityCalendar>();
				var currentDate = request.StartDate;

				while (currentDate <= request.EndDate)
				{
					// Check if entry already exists
					var exists = await _availabilityCalendarRepository.ExistsAsync(request.HomestayId, currentDate);

					if (!exists)
					{
						var calendar = new AvailabilityCalendar
						{
							HomestayId = request.HomestayId,
							AvailableDate = currentDate,
							IsAvailable = request.IsAvailable,
							CustomPrice = request.CustomPrice,
							MinimumNights = request.MinimumNights,
							IsBlocked = request.IsBlocked,
							BlockReason = request.BlockReason,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow
						};

						calendars.Add(calendar);
					}

					currentDate = currentDate.AddDays(1);
				}

				if (calendars.Any())
				{
					await _availabilityCalendarRepository.AddRangeAsync(calendars);
					await _availabilityCalendarRepository.SaveChangesAsync();
				}

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Successfully created {Count} availability calendar entries.", calendars.Count);

				var result = await _availabilityCalendarRepository.GetByDateRangeAsync(
					request.HomestayId, request.StartDate, request.EndDate);

				return _mapper.Map<List<AvailabilityCalendarDto>>(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while bulk creating availability calendar entries.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<AvailabilityCalendarDto?> UpdateAsync(int ownerId, int calendarId, UpdateAvailabilityCalendarDto request)
		{
			_logger.LogInformation("Updating availability calendar entry {CalendarId}.", calendarId);

			var calendar = await _availabilityCalendarRepository.GetByIdAsync(calendarId);
			if (calendar == null)
			{
				_logger.LogWarning("Availability calendar entry with ID {CalendarId} not found.", calendarId);
				throw new NotFoundException($"Availability calendar entry with ID {calendarId} not found.");
			}

			await ValidateOwnerPermissionAsync(ownerId, calendar.HomestayId);

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Update only provided fields
				if (request.IsAvailable.HasValue)
					calendar.IsAvailable = request.IsAvailable.Value;

				if (request.CustomPrice.HasValue)
					calendar.CustomPrice = request.CustomPrice.Value;

				if (request.MinimumNights.HasValue)
					calendar.MinimumNights = request.MinimumNights.Value;

				if (request.IsBlocked.HasValue)
					calendar.IsBlocked = request.IsBlocked.Value;

				if (request.BlockReason != null)
					calendar.BlockReason = request.BlockReason;

				calendar.UpdatedAt = DateTime.UtcNow;

				_availabilityCalendarRepository.Update(calendar);
				await _availabilityCalendarRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Availability calendar entry {CalendarId} updated successfully.", calendarId);

				return _mapper.Map<AvailabilityCalendarDto>(calendar);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating availability calendar entry {CalendarId}.", calendarId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> BulkUpdateAsync(int ownerId, int homestayId, BulkUpdateAvailabilityCalendarDto request)
		{
			_logger.LogInformation("Bulk updating availability calendar entries for homestay {HomestayId} from {StartDate} to {EndDate}.",
				homestayId, request.StartDate, request.EndDate);

			await ValidateOwnerPermissionAsync(ownerId, homestayId);

			if (request.EndDate < request.StartDate)
			{
				throw new BadRequestException("End date must be after start date.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var calendars = await _availabilityCalendarRepository.GetByDateRangeAsync(
					homestayId, request.StartDate, request.EndDate);

				var existingDates = calendars.Select(c => c.AvailableDate).ToHashSet();
				var currentDate = request.StartDate;
				var newCalendars = new List<AvailabilityCalendar>();

				while (currentDate <= request.EndDate)
				{
					if (existingDates.Contains(currentDate))
					{
						// Update existing
						var calendar = calendars.First(c => c.AvailableDate == currentDate);

						if (request.IsAvailable.HasValue)
							calendar.IsAvailable = request.IsAvailable.Value;

						if (request.CustomPrice.HasValue)
							calendar.CustomPrice = request.CustomPrice.Value;

						if (request.MinimumNights.HasValue)
							calendar.MinimumNights = request.MinimumNights.Value;

						if (request.IsBlocked.HasValue)
							calendar.IsBlocked = request.IsBlocked.Value;

						if (request.BlockReason != null)
							calendar.BlockReason = request.BlockReason;

						calendar.UpdatedAt = DateTime.UtcNow; 
					}
					else
					{
						// Create new entry
						var newCalendar = new AvailabilityCalendar
						{
							HomestayId = homestayId,
							AvailableDate = currentDate,
							IsAvailable = request.IsAvailable ?? true,
							CustomPrice = request.CustomPrice,
							MinimumNights = request.MinimumNights,
							IsBlocked = request.IsBlocked ?? false,
							BlockReason = request.BlockReason,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow
						};
						newCalendars.Add(newCalendar);
					}

					currentDate = currentDate.AddDays(1);
				}

				if (newCalendars.Any())
				{
					await _availabilityCalendarRepository.AddRangeAsync(newCalendars);
				}

				await _availabilityCalendarRepository.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Successfully bulk updated availability calendar entries for homestay {HomestayId}.", homestayId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while bulk updating availability calendar entries.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> DeleteAsync(int ownerId, int calendarId)
		{
			_logger.LogInformation("Deleting availability calendar entry {CalendarId}.", calendarId);

			var calendar = await _availabilityCalendarRepository.GetByIdAsync(calendarId);
			if (calendar == null)
			{
				_logger.LogWarning("Availability calendar entry with ID {CalendarId} not found.", calendarId);
				throw new NotFoundException($"Availability calendar entry with ID {calendarId} not found.");
			}

			await ValidateOwnerPermissionAsync(ownerId, calendar.HomestayId);

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				_availabilityCalendarRepository.Remove(calendar);
				await _availabilityCalendarRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Availability calendar entry {CalendarId} deleted successfully.", calendarId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting availability calendar entry {CalendarId}.", calendarId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<AvailabilityCalendarDto?> GetByIdAsync(int calendarId)
		{
			var calendar = await _availabilityCalendarRepository.GetByIdAsync(calendarId);
			if (calendar == null)
			{
				_logger.LogWarning("Availability calendar entry with ID {CalendarId} not found.", calendarId);
				throw new NotFoundException($"Availability calendar entry with ID {calendarId} not found.");
			}

			return _mapper.Map<AvailabilityCalendarDto>(calendar);
		}

		public async Task<AvailabilityCalendarDto?> GetByHomestayAndDateAsync(int homestayId, DateOnly date)
		{
			var calendar = await _availabilityCalendarRepository.GetByHomestayAndDateAsync(homestayId, date);
			return calendar == null ? null : _mapper.Map<AvailabilityCalendarDto>(calendar);
		}

		public async Task<PagedResult<AvailabilityCalendarDto>> GetCalendarWithFilterAsync(AvailabilityCalendarFilter filter)
		{
			var pagedCalendars = await _availabilityCalendarRepository.GetCalendarWithFilterAsync(filter);
			var calendarDtos = _mapper.Map<List<AvailabilityCalendarDto>>(pagedCalendars.Items);

			return new PagedResult<AvailabilityCalendarDto>
			{
				Items = calendarDtos,
				TotalCount = pagedCalendars.TotalCount,
				PageSize = pagedCalendars.PageSize,
				PageNumber = pagedCalendars.PageNumber
			};
		}

		public async Task<IEnumerable<AvailabilityCalendarDto>> GetByDateRangeAsync(int homestayId, DateOnly startDate, DateOnly endDate)
		{
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			if (endDate < startDate)
			{
				throw new BadRequestException("End date must be after start date.");
			}

			var calendars = await _availabilityCalendarRepository.GetByDateRangeAsync(homestayId, startDate, endDate);
			return _mapper.Map<IEnumerable<AvailabilityCalendarDto>>(calendars);
		}

		public async Task<DateRangeAvailabilityDto> CheckDateRangeAvailabilityAsync(int homestayId, DateOnly startDate, DateOnly endDate)
		{
			_logger.LogInformation("Checking availability for homestay {HomestayId} from {StartDate} to {EndDate}.",
				homestayId, startDate, endDate);

			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			if (endDate < startDate)
			{
				throw new BadRequestException("End date must be after start date.");
			}

			var calendars = await _availabilityCalendarRepository.GetByDateRangeAsync(homestayId, startDate, endDate);
			var calendarDict = calendars.ToDictionary(c => c.AvailableDate);

			var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
			var dateDetails = new List<DateAvailabilityDto>();
			var currentDate = startDate;
			var totalPrice = 0m;
			var availableDays = 0;
			var blockedDays = 0;
			var isFullyAvailable = true;

			while (currentDate <= endDate)
			{
				var calendar = calendarDict.ContainsKey(currentDate) ? calendarDict[currentDate] : null;

				bool isAvailable;
				bool isBlocked;
				decimal price;
				int? minimumNights;
				string? blockReason;

				if (calendar != null)
				{
					isAvailable = calendar.IsAvailable && !calendar.IsBlocked;
					isBlocked = calendar.IsBlocked;
					price = calendar.CustomPrice ?? homestay.BaseNightlyPrice;
					minimumNights = calendar.MinimumNights;
					blockReason = calendar.BlockReason;
				}
				else
				{
					// Default behavior if no calendar entry exists
					isAvailable = true;
					isBlocked = false;
					price = homestay.BaseNightlyPrice;
					minimumNights = homestay.MinimumNights;
					blockReason = null;
				}

				if (isAvailable)
				{
					availableDays++;
					totalPrice += price;
				}
				else
				{
					isFullyAvailable = false;
				}

				if (isBlocked)
				{
					blockedDays++;
				}

				dateDetails.Add(new DateAvailabilityDto
				{
					Date = currentDate,
					IsAvailable = isAvailable,
					IsBlocked = isBlocked,
					Price = price,
					MinimumNights = minimumNights,
					BlockReason = blockReason
				});

				currentDate = currentDate.AddDays(1);
			}

			return new DateRangeAvailabilityDto
			{
				StartDate = startDate,
				EndDate = endDate,
				IsAvailable = isFullyAvailable,
				TotalDays = totalDays,
				AvailableDays = availableDays,
				BlockedDays = blockedDays,
				TotalPrice = totalPrice,
				DateDetails = dateDetails
			};
		}

		public async Task<CalendarMonthDto> GetMonthCalendarAsync(int homestayId, int year, int month)
		{
			_logger.LogInformation("Getting calendar for homestay {HomestayId} for {Year}-{Month}.", homestayId, year, month);

			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			if (month < 1 || month > 12)
			{
				throw new BadRequestException("Month must be between 1 and 12.");
			}

			var startDate = new DateOnly(year, month, 1);
			var endDate = startDate.AddMonths(1).AddDays(-1);

			var calendars = await _availabilityCalendarRepository.GetByDateRangeAsync(homestayId, startDate, endDate);
			var calendarDtos = _mapper.Map<List<AvailabilityCalendarDto>>(calendars);

			return new CalendarMonthDto
			{
				Year = year,
				Month = month,
				Dates = calendarDtos
			};
		}

		public async Task<bool> BlockDatesAsync(int ownerId, int homestayId, BlockDatesDto request)
		{
			_logger.LogInformation("Blocking dates for homestay {HomestayId} from {StartDate} to {EndDate}.",
				homestayId, request.StartDate, request.EndDate);

			await ValidateOwnerPermissionAsync(ownerId, homestayId);

			if (request.EndDate < request.StartDate)
			{
				throw new BadRequestException("End date must be after start date.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var calendars = await _availabilityCalendarRepository.GetByDateRangeAsync(
					homestayId, request.StartDate, request.EndDate);

				var existingDates = calendars.ToDictionary(c => c.AvailableDate);
				var currentDate = request.StartDate;
				var newCalendars = new List<AvailabilityCalendar>();

				while (currentDate <= request.EndDate)
				{
					if (existingDates.ContainsKey(currentDate))
					{
						// Update existing
						var calendar = existingDates[currentDate];
						calendar.IsBlocked = true;
						calendar.IsAvailable = false;
						calendar.BlockReason = request.BlockReason;
						calendar.UpdatedAt = DateTime.UtcNow;
						_availabilityCalendarRepository.Update(calendar);
					}
					else
					{
						// Create new blocked entry
						var newCalendar = new AvailabilityCalendar
						{
							HomestayId = homestayId,
							AvailableDate = currentDate,
							IsAvailable = false,
							IsBlocked = true,
							BlockReason = request.BlockReason,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow
						};
						newCalendars.Add(newCalendar);
					}

					currentDate = currentDate.AddDays(1);
				}

				if (newCalendars.Any())
				{
					await _availabilityCalendarRepository.AddRangeAsync(newCalendars);
				}

				await _availabilityCalendarRepository.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Successfully blocked dates for homestay {HomestayId}.", homestayId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while blocking dates.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> UnblockDatesAsync(int ownerId, int homestayId, DateOnly startDate, DateOnly endDate)
		{
			_logger.LogInformation("Unblocking dates for homestay {HomestayId} from {StartDate} to {EndDate}.",
				homestayId, startDate, endDate);

			await ValidateOwnerPermissionAsync(ownerId, homestayId);

			if (endDate < startDate)
			{
				throw new BadRequestException("End date must be after start date.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var calendars = await _availabilityCalendarRepository.GetByDateRangeAsync(
					homestayId, startDate, endDate);

				foreach (var calendar in calendars.Where(c => c.IsBlocked))
				{
					calendar.IsBlocked = false;
					calendar.IsAvailable = true;
					calendar.BlockReason = null;
					calendar.UpdatedAt = DateTime.UtcNow;
					_availabilityCalendarRepository.Update(calendar);
				}

				await _availabilityCalendarRepository.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Successfully unblocked dates for homestay {HomestayId}.", homestayId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while unblocking dates.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}
	}
}
