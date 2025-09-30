using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities
{
	public class Payment : BaseEntity
	{
		public decimal PaymentAmount { get; set; }

		public PaymentMethod PaymentMethod { get; set; }

		public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

		public string? TransactionId { get; set; }

		public string? PaymentGatewayId { get; set; }

		public string? PaymentGateway { get; set; }

		public DateTime? ProcessedAt { get; set; }

		public string? PaymentNotes { get; set; }

		public string? FailureReason { get; set; }

		public decimal? RefundAmount { get; set; }

		public DateTime? RefundedAt { get; set; }

		// Foreign Key
		public int BookingId { get; set; }

		// Navigation Property
		public virtual Booking Booking { get; set; } = null!;
	}
}
