namespace BookingSystem.Application.DTOs.PaymentDTO
{
	public class PaymentUrlResponseDto
	{
		public int PaymentId { get; set; }
		public string PaymentUrl { get; set; } = string.Empty;
		public string TransactionReference { get; set; } = string.Empty;
		public DateTime ExpiryTime { get; set; }
	}
}
