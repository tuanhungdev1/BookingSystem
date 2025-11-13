using BookingSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class CreateCouponDto
	{
		[Required(ErrorMessage = "Coupon code is required")]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Coupon code must be between 3 and 50 characters")]
		[RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Coupon code must contain only uppercase letters, numbers, hyphens and underscores")]
		public string CouponCode { get; set; } = string.Empty;

		[Required(ErrorMessage = "Coupon name is required")]
		[StringLength(200, MinimumLength = 3, ErrorMessage = "Coupon name must be between 3 and 200 characters")]
		public string CouponName { get; set; } = string.Empty;

		[StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
		public string? Description { get; set; }

		[Required(ErrorMessage = "Coupon type is required")]
		public CouponType CouponType { get; set; }

		[Required(ErrorMessage = "Discount value is required")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
		public decimal DiscountValue { get; set; }

		[Range(0.01, double.MaxValue, ErrorMessage = "Max discount amount must be greater than 0")]
		public decimal? MaxDiscountAmount { get; set; }

		[Required(ErrorMessage = "Start date is required")]
		[DataType(DataType.Date)]
		public DateTime StartDate { get; set; }

		[Required(ErrorMessage = "End date is required")]
		[DataType(DataType.Date)]
		public DateTime EndDate { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Total usage limit must be at least 1")]
		public int? TotalUsageLimit { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Usage per user must be at least 1")]
		public int? UsagePerUser { get; set; }

		[Range(0.01, double.MaxValue, ErrorMessage = "Minimum booking amount must be greater than 0")]
		public decimal? MinimumBookingAmount { get; set; }

		[Range(1, 365, ErrorMessage = "Minimum nights must be between 1 and 365")]
		public int? MinimumNights { get; set; }

		public bool IsFirstBookingOnly { get; set; } = false;
		public bool IsNewUserOnly { get; set; } = false;

		[Required(ErrorMessage = "Scope is required")]
		public CouponScope Scope { get; set; } = CouponScope.AllHomestays;

		public int? SpecificHomestayId { get; set; }
		public List<int>? ApplicableHomestayIds { get; set; }
		public bool IsPublic { get; set; } = true;

		[Range(0, 100, ErrorMessage = "Priority must be between 0 and 100")]
		public int Priority { get; set; } = 0;

		public string? ActingAsRole { get; set; }
	}
}
