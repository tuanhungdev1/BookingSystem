using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Enums
{
	public enum CouponType
	{
		Percentage = 1,      // Giảm theo %
		FixedAmount = 2,     // Giảm số tiền cố định
		FirstBooking = 3,    // Ưu đãi booking đầu
		Seasonal = 4,        // Giảm giá theo mùa
		LongStay = 5,        // Giảm giá ở lâu
		Referral = 6         // Giảm giá giới thiệu
	}
}
