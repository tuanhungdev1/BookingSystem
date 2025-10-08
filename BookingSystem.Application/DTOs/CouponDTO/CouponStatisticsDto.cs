namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class CouponStatisticsDto
	{
		public int TotalCoupons { get; set; }
		public int ActiveCoupons { get; set; }
		public int ExpiredCoupons { get; set; }
		public int TotalUsageCount { get; set; }
		public decimal TotalDiscountAmount { get; set; }
		public decimal AverageDiscountAmount { get; set; }
		public List<CouponTypeStatistic> CouponTypeStats { get; set; } = new();
	}
}
