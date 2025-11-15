using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Repositories
{
	public interface ICouponRepository : IRepository<Coupon>
	{
		Task<Coupon?> GetByCodeAsync(string couponCode);
		Task<Coupon?> GetByIdWithDetailsAsync(int id);
		Task<PagedResult<Coupon>> GetAllCouponsAsync(CouponFilter filter);
		Task<PagedResult<Coupon>> GetPublicCouponsAsync(CouponFilter filter);
		Task<IEnumerable<Coupon>> GetActiveCouponsForHomestayAsync(int homestayId);
		Task<IEnumerable<Coupon>> GetUserAvailableCouponsAsync(int userId, int homestayId, decimal bookingAmount, int nights);
		Task<bool> IsCouponCodeUniqueAsync(string couponCode, int? excludeCouponId = null);
		Task<int> GetUserCouponUsageCountAsync(int couponId, int userId);
		Task<bool> CanUserUseCouponAsync(int couponId, int userId);
		Task<CouponUsage?> GetCouponUsageByBookingAsync(int bookingId);
		Task<IEnumerable<CouponUsage>> GetCouponUsageHistoryAsync(int couponId);

		Task<List<CouponUsage>> GetAllCouponUsagesByCouponIdsAsync(List<int> couponIds);
		Task<List<CouponHomestay>> GetCouponHomestaysByCouponIdAsync(int couponId);
		Task RemoveCouponHomestaysAsync(List<CouponHomestay> couponHomestays);
		Task AddCouponHomestaysAsync(List<CouponHomestay> couponHomestays);
		Task<List<Coupon>> GetExpiredActiveCouponsAsync();

		Task<IEnumerable<Coupon>> GetApplicableCouponsForHomestayAsync(
		int homestayId,
		bool includeInactive = false);
	}
}
