using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities
{
	public class BookingGuest : BaseEntity
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime? DateOfBirth { get; set; }
		public Gender? Gender { get; set; }
		public string? IdentityNumber { get; set; }
		public string? IdentityType { get; set; } // Passport, ID Card, etc.
		public string? Nationality { get; set; }
		public Guid BookingId { get; set; }

		// Navigation Properties
		public virtual Booking Booking { get; set; } = null!;
	}
}
