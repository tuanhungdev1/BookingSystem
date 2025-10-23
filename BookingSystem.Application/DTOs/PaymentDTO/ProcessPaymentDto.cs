using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.PaymentDTO
{
	public class ProcessPaymentDto
	{
		[Required(ErrorMessage = "Payment ID is required")]
		public int PaymentId { get; set; }

		[Required(ErrorMessage = "Transaction ID is required")]
		public string TransactionId { get; set; } = string.Empty;

		public string? PaymentGatewayId { get; set; }

		public string? PaymentGateway { get; set; }
	}
}
