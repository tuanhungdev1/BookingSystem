using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class RoomType : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public decimal BasePrice { get; set; }
		public int MaxOccupancy { get; set; }
		public decimal Size { get; set; } // in square meters
		public int BedCount { get; set; }
		public string BedType { get; set; } = string.Empty; // Single, Double, Queen, King
		public bool HasBalcony { get; set; }
		public bool HasSeaView { get; set; }
		public string? MainImage { get; set; }
		public Guid HotelId { get; set; }

		// Navigation Properties
		public virtual Accommodation Accommodation { get; set; } = null!;
		public virtual ICollection<Room> Rooms { get; set; } = new HashSet<Room>();
		public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new HashSet<RoomTypeAmenity>();
		public virtual ICollection<RoomTypeImage> RoomTypeImages { get; set; } = new HashSet<RoomTypeImage>();
		public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
	}
}
