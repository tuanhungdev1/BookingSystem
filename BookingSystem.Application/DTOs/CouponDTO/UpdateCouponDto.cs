using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class UpdateCouponDto
	{
		public string? CouponName { get; set; }
		public string? Description { get; set; }
		public decimal? DiscountValue { get; set; }
		public decimal? MaxDiscountAmount { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int? TotalUsageLimit { get; set; }
		public int? UsagePerUser { get; set; }
		public decimal? MinimumBookingAmount { get; set; }
		public CouponScope? Scope { get; set; } 
		public int? MinimumNights { get; set; }
		public bool? IsPublic { get; set; }
		public int? Priority { get; set; }
		public int? SpecificHomestayId { get; set; }
		public List<int>? ApplicableHomestayIds { get; set; }
		public string? ActingAsRole { get; set; }
	}
}
