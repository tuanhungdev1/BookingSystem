using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities
{
	public class Accommodation : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
		public string PostalCode { get; set; } = string.Empty;
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public string Phone { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? Website { get; set; }
		public int StarRating { get; set; } = 1; // 1-5 stars
		public string? MainImage { get; set; }
		public AccommodationType Type { get; set; } = AccommodationType.Hotel;
		public bool IsActive { get; set; } = true;

		// Navigation Properties
		public virtual ICollection<RoomType> RoomTypes { get; set; } = new HashSet<RoomType>();
		public virtual ICollection<Room> Rooms { get; set; } = new HashSet<Room>();
		public virtual ICollection<AccommodationAmenity> HotelAmenities { get; set; } = new HashSet<AccommodationAmenity>();
		public virtual ICollection<AccommodationImage> HotelImages { get; set; } = new HashSet<AccommodationImage>();
	}
}
