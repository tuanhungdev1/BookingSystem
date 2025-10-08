using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Repositories
{
	public interface ICouponUsageRepository : IRepository<CouponUsage>
	{
		Task<IEnumerable<CouponUsage>> GetByUserIdAsync(int userId);
		Task<IEnumerable<CouponUsage>> GetByCouponIdAsync(int couponId);
		Task<CouponUsage?> GetByBookingIdAsync(int bookingId);
	}
}
