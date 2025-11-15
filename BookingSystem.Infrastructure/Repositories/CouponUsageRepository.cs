using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Infrastructure.Repositories
{
	public class CouponUsageRepository : Repository<CouponUsage>, ICouponUsageRepository
	{
		public CouponUsageRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<List<CouponUsage>> GetCouponUsagesWithCouponByBookingIdAsync(int bookingId)
		{
			return await _context.CouponUsages
				.Include(cu => cu.Coupon)
				.Where(cu => cu.BookingId == bookingId)
				.ToListAsync();
		}

		public void UpdateCouponUsagesRange(IEnumerable<CouponUsage> couponUsages)
		{
			_context.CouponUsages.UpdateRange(couponUsages);
		}

		public async Task<IEnumerable<CouponUsage>> GetAllByBookingIdAsync(int bookingId)
		{
			return await _context.CouponUsages
				.Include(cu => cu.Coupon)
				.Include(cu => cu.User)
				.Where(cu => cu.BookingId == bookingId)
				.OrderByDescending(cu => cu.UsedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<CouponUsage>> GetByUserIdAsync(int userId)
		{
			return await _dbSet
				.Include(cu => cu.Coupon)
				.Include(cu => cu.Booking)
				.Where(cu => cu.UserId == userId)
				.OrderByDescending(cu => cu.UsedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<CouponUsage>> GetByCouponIdAsync(int couponId)
		{
			return await _dbSet
				.Include(cu => cu.User)
				.Include(cu => cu.Booking)
				.Where(cu => cu.CouponId == couponId)
				.OrderByDescending(cu => cu.UsedAt)
				.ToListAsync();
		}

		public async Task<CouponUsage?> GetByBookingIdAsync(int bookingId)
		{
			return await _dbSet
				.Include(cu => cu.Coupon)
				.Include(cu => cu.User)
				.FirstOrDefaultAsync(cu => cu.BookingId == bookingId);
		}
	}
}
