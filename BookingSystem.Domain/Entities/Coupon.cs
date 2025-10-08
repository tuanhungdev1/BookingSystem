using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Entities
{
	public class Coupon : BaseEntity
	{
		[Required]
		[MaxLength(50)]
		public string CouponCode { get; set; } = string.Empty;

		[MaxLength(200)]
		public string CouponName { get; set; } = string.Empty;

		[MaxLength(1000)]
		public string? Description { get; set; }

		public CouponType CouponType { get; set; }

		// Discount value
		public decimal DiscountValue { get; set; } // % or fixed amount
		public decimal? MaxDiscountAmount { get; set; } // Max discount for percentage type

		// Validity
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		// Usage limits
		public int? TotalUsageLimit { get; set; } // Null = unlimited
		public int CurrentUsageCount { get; set; } = 0;
		public int? UsagePerUser { get; set; } // Max times a user can use

		// Conditions
		public decimal? MinimumBookingAmount { get; set; }
		public int? MinimumNights { get; set; }
		public bool IsFirstBookingOnly { get; set; } = false;
		public bool IsNewUserOnly { get; set; } = false;

		// Applicable scope
		public CouponScope Scope { get; set; } = CouponScope.AllHomestays;
		public int? SpecificHomestayId { get; set; }

		// Status
		public bool IsActive { get; set; } = true;
		public bool IsPublic { get; set; } = true; // Public or private (by code only)

		// Priority (higher number = higher priority)
		public int Priority { get; set; } = 0;

		// Foreign Keys
		public int? CreatedByUserId { get; set; }

		// Navigation Properties
		public virtual User? CreatedBy { get; set; }
		public virtual Homestay? SpecificHomestay { get; set; }
		public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
		public virtual ICollection<CouponHomestay> CouponHomestays { get; set; } = new List<CouponHomestay>();
	}
}
