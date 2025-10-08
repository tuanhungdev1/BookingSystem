using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class CreateCouponDto
	{
		public string CouponCode { get; set; } = string.Empty;
		public string CouponName { get; set; } = string.Empty;
		public string? Description { get; set; }
		public CouponType CouponType { get; set; }
		public decimal DiscountValue { get; set; }
		public decimal? MaxDiscountAmount { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int? TotalUsageLimit { get; set; }
		public int? UsagePerUser { get; set; }
		public decimal? MinimumBookingAmount { get; set; }
		public int? MinimumNights { get; set; }
		public bool IsFirstBookingOnly { get; set; } = false;
		public bool IsNewUserOnly { get; set; } = false;
		public CouponScope Scope { get; set; } = CouponScope.AllHomestays;
		public int? SpecificHomestayId { get; set; }
		public List<int>? ApplicableHomestayIds { get; set; }
		public bool IsPublic { get; set; } = true;
		public int Priority { get; set; } = 0;
	}
}
