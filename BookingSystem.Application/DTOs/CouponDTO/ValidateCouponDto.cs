namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class ValidateCouponDto
	{
		public string CouponCode { get; set; } = string.Empty;
		public int HomestayId { get; set; }
		public decimal BookingAmount { get; set; }
		public int NumberOfNights { get; set; }
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
	}
}
