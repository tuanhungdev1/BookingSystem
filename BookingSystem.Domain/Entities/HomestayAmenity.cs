using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class HomestayAmenity : BaseEntity
	{
		public int HomestayId { get; set; }
		public int AmenityId { get; set; }
		public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
		// Cho phép thêm ghi chú tùy chỉnh
		public string? CustomNote { get; set; } // VD: "Free WiFi 100Mbps", "Pool open 6AM-10PM"
		public virtual Homestay Homestay { get; set; }
		public virtual Amenity Amenity { get; set; } // Corrected type from HomestayAmenity to Amenity
	}
}
