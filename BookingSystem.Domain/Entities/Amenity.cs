using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class Amenity : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? Icon { get; set; }
		public string Category { get; set; } = string.Empty; // Hotel, Room, General
		public bool IsActive { get; set; } = true;

		// Navigation Properties
		public virtual ICollection<AccommodationAmenity> HotelAmenities { get; set; } = new HashSet<AccommodationAmenity>();
		public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new HashSet<RoomTypeAmenity>();
	}
}
