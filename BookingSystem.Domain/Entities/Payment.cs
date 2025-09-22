using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities
{
	public class Payment : BaseEntity
	{
		public string PaymentNumber { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public PaymentMethod Method { get; set; }
		public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
		public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
		public string? TransactionId { get; set; }
		public string? Notes { get; set; }
		public Guid BookingId { get; set; }

		// Navigation Properties
		public virtual Booking Booking { get; set; } = null!;
	}
}
