using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;

namespace BookingSystem.Application.Contracts
{
	public interface IAvailabilityCalendarService
	{
		Task<AvailabilityCalendarDto> CreateAsync(int ownerId, CreateAvailabilityCalendarDto request);
		Task<List<AvailabilityCalendarDto>> BulkCreateAsync(int ownerId, BulkCreateAvailabilityCalendarDto request);
		Task<AvailabilityCalendarDto?> UpdateAsync(int ownerId, int calendarId, UpdateAvailabilityCalendarDto request);
		Task<bool> BulkUpdateAsync(int ownerId, int homestayId, BulkUpdateAvailabilityCalendarDto request);
		Task<bool> DeleteAsync(int ownerId, int calendarId);
		Task<bool> DeleteByDateRangeAsync(int ownerId, int homestayId, DateOnly startDate, DateOnly endDate);
		Task<AvailabilityCalendarDto?> GetByIdAsync(int calendarId);
		Task<AvailabilityCalendarDto?> GetByHomestayAndDateAsync(int homestayId, DateOnly date);
		Task<PagedResult<AvailabilityCalendarDto>> GetCalendarWithFilterAsync(AvailabilityCalendarFilter filter);
		Task<IEnumerable<AvailabilityCalendarDto>> GetByDateRangeAsync(int homestayId, DateOnly startDate, DateOnly endDate);
		Task<DateRangeAvailabilityDto> CheckDateRangeAvailabilityAsync(int homestayId, DateOnly startDate, DateOnly endDate);
		Task<CalendarMonthDto> GetMonthCalendarAsync(int homestayId, int year, int month);
		Task<bool> BlockDatesAsync(int ownerId, int homestayId, BlockDatesDto request);
		Task<bool> UnblockDatesAsync(int ownerId, int homestayId, DateOnly startDate, DateOnly endDate);
	}
}
