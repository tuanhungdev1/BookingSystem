using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Enums
{
	public enum CouponScope
	{
		AllHomestays = 1,        // Áp dụng cho tất cả
		SpecificHomestay = 2,    // Áp dụng cho 1 homestay cụ thể
		MultipleHomestays = 3    // Áp dụng cho nhiều homestays
	}
}
