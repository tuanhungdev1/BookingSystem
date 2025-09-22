using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities
{
	public class Booking : BaseEntity
	{
		public string BookingNumber { get; set; } = string.Empty;
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public int NumberOfGuests { get; set; }
		public int NumberOfRooms { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal FinalAmount { get; set; }
		public BookingStatus Status { get; set; } = BookingStatus.Pending;
		public string? SpecialRequests { get; set; }
		public DateTime? CheckInTime { get; set; }
		public DateTime? CheckOutTime { get; set; }
		public Guid UserId { get; set; }
		public Guid RoomTypeId { get; set; }

		// Guest Information (for guests who don't register)
		public string? GuestFirstName { get; set; }
		public string? GuestLastName { get; set; }
		public string? GuestEmail { get; set; }
		public string? GuestPhone { get; set; }

		// Navigation Properties
		public virtual User User { get; set; } = null!;
		public virtual RoomType RoomType { get; set; } = null!;
		public virtual ICollection<BookingRoom> BookingRooms { get; set; } = new HashSet<BookingRoom>();
		public virtual ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
		public virtual ICollection<BookingGuest> BookingGuests { get; set; } = new HashSet<BookingGuest>();
		public virtual Review? Review { get; set; }
	}
}
