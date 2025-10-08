using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class CouponDto
	{
		public int Id { get; set; }
		public string CouponCode { get; set; } = string.Empty;
		public string CouponName { get; set; } = string.Empty;
		public string? Description { get; set; }
		public CouponType CouponType { get; set; }
		public string CouponTypeDisplay { get; set; } = string.Empty;
		public decimal DiscountValue { get; set; }
		public decimal? MaxDiscountAmount { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int? TotalUsageLimit { get; set; }
		public int CurrentUsageCount { get; set; }
		public int? UsagePerUser { get; set; }
		public decimal? MinimumBookingAmount { get; set; }
		public int? MinimumNights { get; set; }
		public bool IsFirstBookingOnly { get; set; }
		public bool IsNewUserOnly { get; set; }
		public CouponScope Scope { get; set; }
		public string ScopeDisplay { get; set; } = string.Empty;
		public int? SpecificHomestayId { get; set; }
		public string? SpecificHomestayName { get; set; }
		public List<int> ApplicableHomestayIds { get; set; } = new();
		public bool IsActive { get; set; }
		public bool IsPublic { get; set; }
		public int Priority { get; set; }
		public bool IsExpired { get; set; }
		public bool IsAvailable { get; set; }
		public int? CreatedByUserId { get; set; }
		public string? CreatedByUserName { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
