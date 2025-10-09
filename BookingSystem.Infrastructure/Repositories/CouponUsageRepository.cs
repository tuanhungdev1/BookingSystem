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
