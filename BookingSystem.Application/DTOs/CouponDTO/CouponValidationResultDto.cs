namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class CouponValidationResultDto
	{
		public bool IsValid { get; set; }
		public string Message { get; set; } = string.Empty;
		public CouponDto? Coupon { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal FinalAmount { get; set; }
	}
}
