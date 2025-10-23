namespace BookingSystem.Application.DTOs.PaymentGatewayDTO
{
	public class PaymentGatewayRequest
	{
		public int BookingId { get; set; }
		public decimal Amount { get; set; }
		public string OrderInfo { get; set; } = string.Empty;
		public string ReturnUrl { get; set; } = string.Empty;
		public string IpAddress { get; set; } = string.Empty;
		public DateTime CreateDate { get; set; } = DateTime.UtcNow;
	}
}
