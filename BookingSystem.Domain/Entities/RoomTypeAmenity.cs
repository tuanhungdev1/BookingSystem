namespace BookingSystem.Domain.Entities
{
	public class RoomTypeAmenity
	{
		public Guid RoomTypeId { get; set; }
		public Guid AmenityId { get; set; }
		public DateTime AddedAt { get; set; } = DateTime.UtcNow;

		// Navigation Properties
		public virtual RoomType RoomType { get; set; } = null!;
		public virtual Amenity Amenity { get; set; } = null!;
	}
}
