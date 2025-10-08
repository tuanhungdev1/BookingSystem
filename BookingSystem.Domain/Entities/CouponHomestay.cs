using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Entities
{
	public class CouponHomestay
	{
		public int CouponId { get; set; }
		public int HomestayId { get; set; }

		public virtual Coupon Coupon { get; set; } = null!;
		public virtual Homestay Homestay { get; set; } = null!;
	}
}
