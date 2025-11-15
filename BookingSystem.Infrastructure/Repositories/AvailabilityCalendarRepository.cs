using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
    public class AvailabilityCalendarRepository : Repository<AvailabilityCalendar>, IAvailabilityCalendarRepository
	{
		public AvailabilityCalendarRepository(BookingDbContext context) : base(context) {  }

		public async Task<AvailabilityCalendar?> GetByHomestayAndDateAsync(int homestayId, DateOnly date)
		{
			return await _dbSet
				.Include(ac => ac.Homestay)
				.FirstOrDefaultAsync(ac => ac.HomestayId == homestayId && ac.AvailableDate == date);
		}

		// Thêm vào class AvailabilityCalendarRepository
		public async Task<IEnumerable<AvailabilityCalendar>> GetByIdsAsync(IEnumerable<int> ids)
		{
			return await _dbSet
				.Where(ac => ids.Contains(ac.Id) && !ac.IsDeleted)
				.ToListAsync();
		}

		public async Task<IEnumerable<AvailabilityCalendar>> GetByHomestayIdAsync(int homestayId)
		{
			return await _dbSet
				.Where(ac => ac.HomestayId == homestayId)
				.OrderBy(ac => ac.AvailableDate)
				.ToListAsync();
		}

		public async Task<IEnumerable<AvailabilityCalendar>> GetByDateRangeAsync(int homestayId, DateOnly startDate, DateOnly endDate)
		{
			return await _dbSet
				.Where(ac => ac.HomestayId == homestayId
					&& ac.AvailableDate >= startDate
					&& ac.AvailableDate <= endDate)
				.OrderBy(ac => ac.AvailableDate)
				.ToListAsync();
		}

		public async Task<IEnumerable<AvailabilityCalendar>> GetAvailableDatesAsync(int homestayId, DateOnly startDate, DateOnly endDate)
		{
			return await _dbSet
				.Where(ac => ac.HomestayId == homestayId
					&& ac.AvailableDate >= startDate
					&& ac.AvailableDate <= endDate
					&& ac.IsAvailable
					&& !ac.IsBlocked)
				.OrderBy(ac => ac.AvailableDate)
				.ToListAsync();
		}

		public async Task<IEnumerable<AvailabilityCalendar>> GetBlockedDatesAsync(int homestayId, DateOnly startDate, DateOnly endDate)
		{
			return await _dbSet
				.Where(ac => ac.HomestayId == homestayId
					&& ac.AvailableDate >= startDate
					&& ac.AvailableDate <= endDate
					&& ac.IsBlocked)
				.OrderBy(ac => ac.AvailableDate)
				.ToListAsync();
		}

		public async Task<bool> IsDateAvailableAsync(int homestayId, DateOnly date)
		{
			var calendar = await _dbSet
				.FirstOrDefaultAsync(ac => ac.HomestayId == homestayId && ac.AvailableDate == date);

			// If no calendar entry exists, consider it available (default behavior)
			if (calendar == null)
				return true;

			return calendar.IsAvailable && !calendar.IsBlocked;
		}

		public async Task<bool> IsDateRangeAvailableAsync(
	int homestayId,
	DateOnly startDate,
	DateOnly endDate,
	string excludeBookingCode = null) // ✅ THÊM
		{
			var calendars = await _dbSet
				.Where(ac => ac.HomestayId == homestayId
					&& ac.AvailableDate >= startDate
					&& ac.AvailableDate <= endDate)
				.ToListAsync();

			var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
			var allDatesInRange = Enumerable.Range(0, totalDays)
				.Select(offset => startDate.AddDays(offset))
				.ToList();

			foreach (var date in allDatesInRange)
			{
				var calendar = calendars.FirstOrDefault(c => c.AvailableDate == date);

				if (calendar != null)
				{
					// ✅ THÊM: Nếu bị block bởi chính booking đang update, bỏ qua
					if (!string.IsNullOrEmpty(excludeBookingCode) &&
						calendar.BlockReason != null &&
						calendar.BlockReason.Contains(excludeBookingCode))
					{
						continue; // Bỏ qua ngày này
					}

					// Check không available hoặc bị block
					if (!calendar.IsAvailable || calendar.IsBlocked)
						return false;
				}
			}

			return true;
		}

		public async Task<int> CountAvailableDaysAsync(int homestayId, DateOnly startDate, DateOnly endDate)
		{
			var calendars = await _dbSet
				.Where(ac => ac.HomestayId == homestayId
					&& ac.AvailableDate >= startDate
					&& ac.AvailableDate <= endDate)
				.ToListAsync();

			var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
			var allDatesInRange = Enumerable.Range(0, totalDays)
				.Select(offset => startDate.AddDays(offset))
				.ToList();

			var availableCount = 0;
			foreach (var date in allDatesInRange)
			{
				var calendar = calendars.FirstOrDefault(c => c.AvailableDate == date);

				// If no calendar entry or calendar is available and not blocked
				if (calendar == null || (calendar.IsAvailable && !calendar.IsBlocked))
					availableCount++;
			}

			return availableCount;
		}

		public async Task<PagedResult<AvailabilityCalendar>> GetCalendarWithFilterAsync(AvailabilityCalendarFilter filter)
		{
			var query = _dbSet
				.Include(ac => ac.Homestay)
				.AsQueryable();

			// Apply filters
			if (filter.HomestayId.HasValue)
			{
				query = query.Where(ac => ac.HomestayId == filter.HomestayId.Value);
			}

			if (filter.StartDate.HasValue)
			{
				query = query.Where(ac => ac.AvailableDate >= filter.StartDate.Value);
			}

			if (filter.EndDate.HasValue)
			{
				query = query.Where(ac => ac.AvailableDate <= filter.EndDate.Value);
			}

			if (filter.IsAvailable.HasValue)
			{
				query = query.Where(ac => ac.IsAvailable == filter.IsAvailable.Value);
			}

			if (filter.IsBlocked.HasValue)
			{
				query = query.Where(ac => ac.IsBlocked == filter.IsBlocked.Value);
			}

			if (filter.HasCustomPrice.HasValue)
			{
				if (filter.HasCustomPrice.Value)
					query = query.Where(ac => ac.CustomPrice.HasValue);
				else
					query = query.Where(ac => !ac.CustomPrice.HasValue);
			}

			// Get total count
			var totalCount = await query.CountAsync();

			// Apply sorting
			query = filter.SortBy?.ToLower() switch
			{
				"date" => filter.SortOrder.ToLower() == "desc"
					? query.OrderByDescending(ac => ac.AvailableDate)
					: query.OrderBy(ac => ac.AvailableDate),
				"price" => filter.SortOrder.ToLower() == "desc"
					? query.OrderByDescending(ac => ac.CustomPrice)
					: query.OrderBy(ac => ac.CustomPrice),
				_ => query.OrderBy(ac => ac.AvailableDate)
			};

			// Apply pagination
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<AvailabilityCalendar>(items, totalCount, filter.PageNumber, filter.PageSize);
		}

		public async Task<bool> ExistsAsync(int homestayId, DateOnly date)
		{
			return await _dbSet.AnyAsync(ac => ac.HomestayId == homestayId && ac.AvailableDate == date);
		}

		public async Task<List<DateOnly>> GetExistingDatesAsync(int homestayId, List<DateOnly> dates)
		{
			if (dates == null || dates.Count == 0)
				return new List<DateOnly>();

			// Chuẩn hóa danh sách ngày (loại bỏ trùng lặp)
			var normalizedDates = dates.Distinct().ToList();

			// Truy vấn các ngày đã tồn tại trong DB
			return await _context.AvailabilityCalendars
				.AsNoTracking()
				.Where(ac => ac.HomestayId == homestayId && !ac.IsDeleted &&
							 normalizedDates.Contains(ac.AvailableDate))
				.Select(ac => ac.AvailableDate)
				.Distinct()
				.ToListAsync();
		}


		public async Task DeleteByDateRangeAsync(int homestayId, DateOnly startDate, DateOnly endDate)
		{
			var calendarsToDelete = await _dbSet
				.Where(ac => ac.HomestayId == homestayId
					&& ac.AvailableDate >= startDate
					&& ac.AvailableDate <= endDate)
				.ToListAsync();

			_dbSet.RemoveRange(calendarsToDelete);
		}
	}
}
