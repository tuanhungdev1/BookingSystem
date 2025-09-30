using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class Amenity : BaseEntity
	{
		public string AmenityName { get; set; } = string.Empty;

		public string? AmenityDescription { get; set; }

		public string? IconUrl { get; set; }

		public string Category { get; set; } = string.Empty;

		public bool IsPopular { get; set; } = false;

		public bool IsActive { get; set; } = true;

		public int DisplayOrder { get; set; } = 0;

		// Navigation Properties
		public virtual ICollection<HomestayAmenity> HomestayAmenities { get; set; } = new List<HomestayAmenity>();
	}
}
