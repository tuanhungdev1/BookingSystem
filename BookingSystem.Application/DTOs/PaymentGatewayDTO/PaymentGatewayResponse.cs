namespace BookingSystem.Application.DTOs.PaymentGatewayDTO
{
	public class PaymentGatewayResponse
	{
		public bool Success { get; set; }
		public string PaymentUrl { get; set; } = string.Empty;
		public string TransactionId { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public Dictionary<string, string>? Data { get; set; }
	}
}
