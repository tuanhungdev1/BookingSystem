namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class CouponUsageDto
	{
		public int Id { get; set; }
		public int CouponId { get; set; }
		public string CouponCode { get; set; } = string.Empty;
		public string CouponName { get; set; } = string.Empty; 
		public int UserId { get; set; }
		public string UserName { get; set; } = string.Empty;
		public int BookingId { get; set; }
		public string BookingCode { get; set; } = string.Empty;
		public decimal DiscountAmount { get; set; }
		public DateTime UsedAt { get; set; }
		public CouponDto? Coupon { get; set; }
	}
}
