namespace BookingSystem.Application.DTOs.PaymentGatewayDTO
{
	public class PaymentGatewayCallback
	{
		public bool Success { get; set; }
		public string TransactionId { get; set; } = string.Empty;
		public string OrderId { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public string ResponseCode { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public DateTime TransactionDate { get; set; }
		public string BankCode { get; set; } = string.Empty;
		public Dictionary<string, string> RawData { get; set; } = new();
	}
}
