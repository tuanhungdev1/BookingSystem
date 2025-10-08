using BookingSystem.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Entities
{
	// Track coupon usage
	public class CouponUsage : BaseEntity
	{
		public int CouponId { get; set; }
		public int UserId { get; set; }
		public int BookingId { get; set; }
		public decimal DiscountAmount { get; set; }
		public DateTime UsedAt { get; set; }

		// Navigation Properties
		public virtual Coupon Coupon { get; set; } = null!;
		public virtual User User { get; set; } = null!;
		public virtual Booking Booking { get; set; } = null!;
	}
}
