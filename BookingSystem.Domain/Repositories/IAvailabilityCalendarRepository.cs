using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Domain.Repositories
{
	public interface IAvailabilityCalendarRepository : IRepository<AvailabilityCalendar>
	{
		Task<AvailabilityCalendar?> GetByHomestayAndDateAsync(int homestayId, DateOnly date);
		Task<IEnumerable<AvailabilityCalendar>> GetByHomestayIdAsync(int homestayId);
		Task<IEnumerable<AvailabilityCalendar>> GetByDateRangeAsync(int homestayId, DateOnly startDate, DateOnly endDate);
		Task<IEnumerable<AvailabilityCalendar>> GetAvailableDatesAsync(int homestayId, DateOnly startDate, DateOnly endDate);
		Task<IEnumerable<AvailabilityCalendar>> GetBlockedDatesAsync(int homestayId, DateOnly startDate, DateOnly endDate);
		Task<bool> IsDateAvailableAsync(int homestayId, DateOnly date);
		Task<bool> IsDateRangeAvailableAsync(
	int homestayId,
	DateOnly startDate,
	DateOnly endDate,
	string excludeBookingCode = null);
		Task<int> CountAvailableDaysAsync(int homestayId, DateOnly startDate, DateOnly endDate);
		Task<PagedResult<AvailabilityCalendar>> GetCalendarWithFilterAsync(AvailabilityCalendarFilter filter);
		Task<bool> ExistsAsync(int homestayId, DateOnly date);
		Task DeleteByDateRangeAsync(int homestayId, DateOnly startDate, DateOnly endDate);
		Task<List<DateOnly>> GetExistingDatesAsync(int homestayId, List<DateOnly> dates);
		Task<IEnumerable<AvailabilityCalendar>> GetByIdsAsync(IEnumerable<int> ids);
	}
}