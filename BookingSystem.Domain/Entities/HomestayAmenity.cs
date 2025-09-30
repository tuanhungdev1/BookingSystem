namespace BookingSystem.Domain.Entities
{
	public class HomestayAmenity
	{
		public int HomestayId { get; set; }
		public int AmenityId { get; set; }
		public DateTime AssignedAt { get; set; }
		public virtual Homestay Homestay { get; set; }
		public virtual Amenity Amenity { get; set; } // Corrected type from HomestayAmenity to Amenity
	}
}
