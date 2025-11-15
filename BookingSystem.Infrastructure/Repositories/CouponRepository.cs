using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
	public class CouponRepository : Repository<Coupon>, ICouponRepository
	{
		public CouponRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<Coupon>> GetApplicableCouponsForHomestayAsync(
	int homestayId,
	bool includeInactive = false)
		{
			var query = _context.Coupons
				.Include(c => c.CouponHomestays)
				.Include(c => c.SpecificHomestay)
				.Include(c => c.CreatedBy)
				.AsQueryable();

			// Lọc theo trạng thái active
			if (!includeInactive)
			{
				query = query.Where(c => c.IsActive);
			}

			// Lấy coupon áp dụng cho homestay này
			var coupons = await query
				.Where(c =>
					// Case 1: Scope = AllHomestays
					c.Scope == CouponScope.AllHomestays ||

					// Case 2: Scope = SpecificHomestay và match homestayId
					(c.Scope == CouponScope.SpecificHomestay && c.SpecificHomestayId == homestayId) ||

					// Case 3: Scope = MultipleHomestays và homestayId có trong list
					(c.Scope == CouponScope.MultipleHomestays &&
					 c.CouponHomestays.Any(ch => ch.HomestayId == homestayId))
				)
				.OrderByDescending(c => c.Priority)
				.ThenBy(c => c.CouponCode)
				.ToListAsync();

			return coupons;
		}

		public async Task<List<CouponUsage>> GetAllCouponUsagesByCouponIdsAsync(List<int> couponIds)
		{
			return await _context.CouponUsages
				.Where(cu => couponIds.Contains(cu.CouponId))
				.ToListAsync();
		}

		public async Task<List<CouponHomestay>> GetCouponHomestaysByCouponIdAsync(int couponId)
		{
			return await _context.CouponHomestays
				.Where(ch => ch.CouponId == couponId)
				.ToListAsync();
		}

		public async Task RemoveCouponHomestaysAsync(List<CouponHomestay> couponHomestays)
		{
			_context.CouponHomestays.RemoveRange(couponHomestays);
			await _context.SaveChangesAsync();
		}

		public async Task AddCouponHomestaysAsync(List<CouponHomestay> couponHomestays)
		{
			await _context.CouponHomestays.AddRangeAsync(couponHomestays);
			await _context.SaveChangesAsync();
		}

		public async Task<List<Coupon>> GetExpiredActiveCouponsAsync()
		{
			var now = DateTime.UtcNow;
			return await _dbSet
				.Where(c => c.IsActive && c.EndDate < now)
				.ToListAsync();
		}

		public async Task<Coupon?> GetByCodeAsync(string couponCode)
		{
			return await _dbSet
				.Include(c => c.CreatedBy)
				.Include(c => c.SpecificHomestay)
				.Include(c => c.CouponHomestays)
					.ThenInclude(ch => ch.Homestay)
				.FirstOrDefaultAsync(c => c.CouponCode.ToUpper() == couponCode.ToUpper());
		}

		public async Task<Coupon?> GetByIdWithDetailsAsync(int id)
		{
			return await _dbSet
				.Include(c => c.CreatedBy)
				.Include(c => c.SpecificHomestay)
				.Include(c => c.CouponHomestays)
					.ThenInclude(ch => ch.Homestay)
				.Include(c => c.CouponUsages)
					.ThenInclude(cu => cu.User)
				.FirstOrDefaultAsync(c => c.Id == id);
		}

		public async Task<PagedResult<Coupon>> GetAllCouponsAsync(CouponFilter filter)
		{
			var query = _dbSet
				.Include(c => c.CreatedBy)
				.Include(c => c.SpecificHomestay)
				.Include(c => c.CouponHomestays)
				.AsQueryable();

			query = ApplyFilters(query, filter);
			query = ApplySorting(query, filter);

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Coupon>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize
			};
		}

		public async Task<PagedResult<Coupon>> GetPublicCouponsAsync(CouponFilter filter)
		{
			filter.IsPublic = true;
			filter.IsActive = true;
			return await GetAllCouponsAsync(filter);
		}

		public async Task<IEnumerable<Coupon>> GetActiveCouponsForHomestayAsync(int homestayId)
		{
			var now = DateTime.UtcNow;

			return await _dbSet
				.Include(c => c.CouponHomestays)
				.Where(c => c.IsActive
					&& c.StartDate <= now
					&& c.EndDate >= now
					&& (c.Scope == CouponScope.AllHomestays
						|| (c.Scope == CouponScope.SpecificHomestay && c.SpecificHomestayId == homestayId)
						|| (c.Scope == CouponScope.MultipleHomestays && c.CouponHomestays.Any(ch => ch.HomestayId == homestayId))))
				.OrderByDescending(c => c.Priority)
				.ThenByDescending(c => c.DiscountValue)
				.ToListAsync();
		}

		public async Task<IEnumerable<Coupon>> GetUserAvailableCouponsAsync(
			int userId,
			int homestayId,
			decimal bookingAmount,
			int nights)
		{
			var now = DateTime.UtcNow;
			var user = await _context.Users.FindAsync(userId);
			if (user == null) return new List<Coupon>();

			var userBookingsCount = await _context.Bookings
				.Where(b => b.GuestId == userId && b.BookingStatus == BookingStatus.Completed)
				.CountAsync();

			var query = _dbSet
				.Include(c => c.CouponUsages)
				.Include(c => c.CouponHomestays)
				.Where(c => c.IsActive
					&& c.StartDate <= now
					&& c.EndDate >= now
					&& (!c.TotalUsageLimit.HasValue || c.CurrentUsageCount < c.TotalUsageLimit.Value)
					&& (!c.MinimumBookingAmount.HasValue || bookingAmount >= c.MinimumBookingAmount.Value)
					&& (!c.MinimumNights.HasValue || nights >= c.MinimumNights.Value)
					&& (!c.IsFirstBookingOnly || userBookingsCount == 0)
					&& (c.Scope == CouponScope.AllHomestays
						|| (c.Scope == CouponScope.SpecificHomestay && c.SpecificHomestayId == homestayId)
						|| (c.Scope == CouponScope.MultipleHomestays && c.CouponHomestays.Any(ch => ch.HomestayId == homestayId))));

			// Check usage per user limit
			var coupons = await query.ToListAsync();
			var availableCoupons = new List<Coupon>();

			foreach (var coupon in coupons)
			{
				if (!coupon.UsagePerUser.HasValue)
				{
					availableCoupons.Add(coupon);
					continue;
				}

				var userUsageCount = coupon.CouponUsages.Count(cu => cu.UserId == userId);
				if (userUsageCount < coupon.UsagePerUser.Value)
				{
					availableCoupons.Add(coupon);
				}
			}

			return availableCoupons
				.OrderByDescending(c => c.Priority)
				.ThenByDescending(c => c.DiscountValue);
		}

		public async Task<bool> IsCouponCodeUniqueAsync(string couponCode, int? excludeCouponId = null)
		{
			var query = _dbSet.Where(c => c.CouponCode.ToUpper() == couponCode.ToUpper());

			if (excludeCouponId.HasValue)
			{
				query = query.Where(c => c.Id != excludeCouponId.Value);
			}

			return !await query.AnyAsync();
		}

		public async Task<int> GetUserCouponUsageCountAsync(int couponId, int userId)
		{
			return await _context.CouponUsages
				.Where(cu => cu.CouponId == couponId && cu.UserId == userId)
				.CountAsync();
		}

		public async Task<bool> CanUserUseCouponAsync(int couponId, int userId)
		{
			var coupon = await GetByIdAsync(couponId);
			if (coupon == null || !coupon.IsActive)
				return false;

			var now = DateTime.UtcNow;
			if (coupon.StartDate > now || coupon.EndDate < now)
				return false;

			// Check total usage limit
			if (coupon.TotalUsageLimit.HasValue && coupon.CurrentUsageCount >= coupon.TotalUsageLimit.Value)
				return false;

			// Check usage per user
			if (coupon.UsagePerUser.HasValue)
			{
				var userUsageCount = await GetUserCouponUsageCountAsync(couponId, userId);
				if (userUsageCount >= coupon.UsagePerUser.Value)
					return false;
			}

			// Check first booking only
			if (coupon.IsFirstBookingOnly)
			{
				var hasCompletedBooking = await _context.Bookings
					.AnyAsync(b => b.GuestId == userId && b.BookingStatus == BookingStatus.Completed);
				if (hasCompletedBooking)
					return false;
			}

			return true;
		}

		public async Task<CouponUsage?> GetCouponUsageByBookingAsync(int bookingId)
		{
			return await _context.CouponUsages
				.Include(cu => cu.Coupon)
				.Include(cu => cu.User)
				.FirstOrDefaultAsync(cu => cu.BookingId == bookingId);
		}

		public async Task<IEnumerable<CouponUsage>> GetCouponUsageHistoryAsync(int couponId)
		{
			return await _context.CouponUsages
				.Include(cu => cu.User)
				.Include(cu => cu.Booking)
				.Where(cu => cu.CouponId == couponId)
				.OrderByDescending(cu => cu.UsedAt)
				.ToListAsync();
		}

		private IQueryable<Coupon> ApplyFilters(IQueryable<Coupon> query, CouponFilter filter)
		{
			if (!string.IsNullOrWhiteSpace(filter.CouponCode))
			{
				query = query.Where(c => c.CouponCode.Contains(filter.CouponCode));
			}

			if (filter.CouponType.HasValue)
			{
				query = query.Where(c => c.CouponType == filter.CouponType.Value);
			}

			if (filter.IsActive.HasValue)
			{
				query = query.Where(c => c.IsActive == filter.IsActive.Value);
			}

			if (filter.IsPublic.HasValue)
			{
				query = query.Where(c => c.IsPublic == filter.IsPublic.Value);
			}

			if (filter.IsExpired.HasValue)
			{
				var now = DateTime.UtcNow;
				query = filter.IsExpired.Value
					? query.Where(c => c.EndDate < now)
					: query.Where(c => c.EndDate >= now);
			}

			if (filter.ValidFrom.HasValue)
			{
				query = query.Where(c => c.StartDate >= filter.ValidFrom.Value);
			}

			if (filter.ValidTo.HasValue)
			{
				query = query.Where(c => c.EndDate <= filter.ValidTo.Value);
			}

			if (filter.HomestayId.HasValue)
			{
				query = query.Where(c =>
					c.Scope == CouponScope.AllHomestays
					|| (c.Scope == CouponScope.SpecificHomestay && c.SpecificHomestayId == filter.HomestayId.Value)
					|| (c.Scope == CouponScope.MultipleHomestays && c.CouponHomestays.Any(ch => ch.HomestayId == filter.HomestayId.Value)));
			}

			if (filter.CreatedByUserId.HasValue)
			{
				query = query.Where(c => c.CreatedByUserId == filter.CreatedByUserId.Value);
			}

			if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
			{
				query = query.Where(c =>
					c.CouponCode.Contains(filter.SearchTerm)
					|| c.CouponName.Contains(filter.SearchTerm)
					|| (c.Description != null && c.Description.Contains(filter.SearchTerm)));
			}

			return query;
		}

		private IQueryable<Coupon> ApplySorting(IQueryable<Coupon> query, CouponFilter filter)
		{
			query = filter.SortBy?.ToLower() switch
			{
				"priority" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(c => c.Priority)
					: query.OrderBy(c => c.Priority),
				"startdate" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(c => c.StartDate)
					: query.OrderBy(c => c.StartDate),
				"enddate" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(c => c.EndDate)
					: query.OrderBy(c => c.EndDate),
				"usage" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(c => c.CurrentUsageCount)
					: query.OrderBy(c => c.CurrentUsageCount),
				_ => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(c => c.CreatedAt)
					: query.OrderBy(c => c.CreatedAt)
			};

			return query;
		}
	}
}