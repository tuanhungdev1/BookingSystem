using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.PaymentDTO
{
	public class RefundPaymentDto
	{
		[Required(ErrorMessage = "Refund amount is required")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
		public decimal RefundAmount { get; set; }

		[Required(ErrorMessage = "Refund reason is required")]
		[MaxLength(500, ErrorMessage = "Refund reason cannot exceed 500 characters")]
		public string RefundReason { get; set; } = string.Empty;
	}
}
