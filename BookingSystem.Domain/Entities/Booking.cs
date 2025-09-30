using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities
{
	public class Booking : BaseEntity
	{
		public DateTime CheckInDate { get; set; }

		public DateTime CheckOutDate { get; set; }

		public int NumberOfGuests { get; set; }

		public int NumberOfAdults { get; set; }

		public int NumberOfChildren { get; set; }

		public int NumberOfInfants { get; set; } = 0;

		public decimal BaseAmount { get; set; }

		public decimal CleaningFee { get; set; } = 0;

		public decimal ServiceFee { get; set; } = 0;

		public decimal TaxAmount { get; set; } = 0;

		public decimal DiscountAmount { get; set; } = 0;

		public decimal TotalAmount { get; set; }

		public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

		public string? SpecialRequests { get; set; }

		public string? CancellationReason { get; set; }

		public DateTime? CancelledAt { get; set; }

		public string? CancelledBy { get; set; }

		public string BookingCode { get; set; } = string.Empty;

		// Foreign Keys
		public int GuestId { get; set; } 

		public int HomestayId { get; set; }

		// Navigation Properties
		public virtual User Guest { get; set; } = null!;
		public virtual Homestay Homestay { get; set; } = null!;
		public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
		public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
	}
}
