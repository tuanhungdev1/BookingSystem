using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.PaymentDTO
{
	public class PaymentDto
	{
		public int Id { get; set; }
		public decimal PaymentAmount { get; set; }
		public PaymentMethod PaymentMethod { get; set; }
		public string PaymentMethodDisplay { get; set; } = string.Empty;
		public PaymentStatus PaymentStatus { get; set; }
		public string PaymentStatusDisplay { get; set; } = string.Empty;
		public string? TransactionId { get; set; }
		public string? PaymentGateway { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? PaymentNotes { get; set; }
		public string? FailureReason { get; set; }
		public decimal? RefundAmount { get; set; }
		public DateTime? RefundedAt { get; set; }
		public DateTime CreatedAt { get; set; }

		public int BookingId { get; set; }

		// Optional: Booking information
		public BookingPaymentInfoDto? Booking { get; set; }
	}
}
