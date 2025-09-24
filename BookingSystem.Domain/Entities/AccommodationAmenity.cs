namespace BookingSystem.Domain.Entities
{
	public class AccommodationAmenity
	{
		public Guid AccommodationId { get; set; }
		public Guid AmenityId { get; set; }
		public DateTime AddedAt { get; set; } = DateTime.UtcNow;

		// Navigation Properties
		public virtual Accommodation Accommodation { get; set; } = null!;
		public virtual Amenity Amenity { get; set; } = null!;
	}
}
