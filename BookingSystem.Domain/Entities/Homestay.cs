using BookingSystem.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Domain.Entities
{
	public class Homestay : BaseEntity
	{
		public string HomestayTitle { get; set; } = string.Empty;

		public string? HomestayDescription { get; set; }

		public string FullAddress { get; set; } = string.Empty;

		public string City { get; set; } = string.Empty;

		public string Province { get; set; } = string.Empty;

		public string Country { get; set; } = "Vietnam";

		public string? PostalCode { get; set; }

		public decimal Latitude { get; set; }

		public decimal Longitude { get; set; }

		public int MaximumGuests { get; set; }

		public int NumberOfBedrooms { get; set; }

		public int NumberOfBathrooms { get; set; }

		public int NumberOfBeds { get; set; }

		public decimal BaseNightlyPrice { get; set; }

		public decimal? WeekendPrice { get; set; }

		public decimal? WeeklyDiscount { get; set; }

		public decimal? MonthlyDiscount { get; set; }

		public int MinimumNights { get; set; } = 1;

		public int MaximumNights { get; set; } = 365;

		public TimeOnly CheckInTime { get; set; } = new TimeOnly(15, 0);

		public TimeOnly CheckOutTime { get; set; } = new TimeOnly(11, 0);

		public bool IsInstantBook { get; set; } = false;

		public bool IsActive { get; set; } = true;

		public bool IsApproved { get; set; } = false;

		public DateTime? ApprovedAt { get; set; }

		[MaxLength(100)]
		public string? ApprovedBy { get; set; }

		public bool IsFeatured { get; set; } = false;

		// Foreign Keys
		[Required]
		public int OwnerId { get; set; } 

		public int PropertyTypeId { get; set; }

		// Navigation Properties
		public virtual User Owner { get; set; } = null!;
		public virtual PropertyType PropertyType { get; set; } = null!;
		public virtual ICollection<HomestayImage> HomestayImages { get; set; } = new List<HomestayImage>();
		public virtual ICollection<HomestayAmenity> HomestayAmenities { get; set; } = new List<HomestayAmenity>();
		public virtual ICollection<HomestayRule> HomestayRules { get; set; } = new List<HomestayRule>();
		public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
		public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
		public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
		public virtual ICollection<AvailabilityCalendar> AvailabilityCalendars { get; set; } = new List<AvailabilityCalendar>();
	}
}
