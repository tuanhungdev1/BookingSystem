using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class CouponTypeStatistic
	{
		public CouponType Type { get; set; }
		public string TypeDisplay { get; set; } = string.Empty;
		public int Count { get; set; }
		public int UsageCount { get; set; }
		public decimal TotalDiscount { get; set; }
	}
}
