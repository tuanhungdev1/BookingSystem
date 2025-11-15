using BookingSystem.Application.DTOs.CouponDTO;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;

namespace BookingSystem.Application.Contracts
{
	public interface ICouponService
	{
		// CRUD Operations
		Task<CouponDto> CreateCouponAsync(int userId, CreateCouponDto request);
		Task<CouponDto?> UpdateCouponAsync(int couponId, int userId, UpdateCouponDto request);
		Task<bool> DeleteCouponAsync(int couponId, int userId);
		Task<CouponDto?> GetByIdAsync(int couponId);
		Task<CouponDto?> GetByCodeAsync(string couponCode);
		Task<PagedResult<CouponDto>> GetAllCouponsAsync(CouponFilter filter);
		Task<PagedResult<CouponDto>> GetPublicCouponsAsync(CouponFilter filter);

		// Coupon Validation & Application
		Task<CouponValidationResultDto> ValidateCouponAsync(int userId, ValidateCouponDto request);
		Task<decimal> CalculateDiscountAmountAsync(string couponCode, decimal bookingAmount, int nights);
		Task<IEnumerable<CouponDto>> GetAvailableCouponsForUserAsync(int userId, int homestayId, decimal bookingAmount, int nights);

		// Coupon Usage
		Task<CouponUsageDto> ApplyCouponToBookingAsync(int bookingId, string couponCode, int userId);
		Task<bool> RemoveCouponFromBookingAsync(int bookingId, int userId, string couponCode);
		Task<IEnumerable<CouponUsageDto>> GetCouponUsagesByBookingAsync(int bookingId);
		Task<IEnumerable<CouponUsageDto>> GetUserCouponUsageHistoryAsync(int userId);
		Task<IEnumerable<CouponUsageDto>> GetCouponUsageHistoryAsync(int couponId);

		// Status Management
		Task<bool> ActivateCouponAsync(int couponId, int userId);
		Task<bool> DeactivateCouponAsync(int couponId, int userId);
		Task<bool> ExtendCouponExpiryAsync(int couponId, int userId, DateTime newEndDate);

		// Statistics & Reporting
		Task<CouponStatisticsDto> GetCouponStatisticsAsync(int? createdByUserId = null);
		Task ProcessExpiredCouponsAsync();

		Task<IEnumerable<CouponDto>> GetApplicableCouponsForHomestayAsync(
		int homestayId,
		bool includeInactive = false);

		Task<IEnumerable<CouponDto>> GetActiveApplicableCouponsForHomestayAsync(
	   int homestayId,
	   int? userId = null,
	   decimal? bookingAmount = null,
	   int? numberOfNights = null);
	}
}
