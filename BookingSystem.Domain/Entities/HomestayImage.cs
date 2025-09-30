using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class HomestayImage : BaseEntity
	{
		public string ImageUrl { get; set; } = string.Empty;

		public string? ImageTitle { get; set; }

		public string? ImageDescription { get; set; }

		public int DisplayOrder { get; set; } = 0;

		public bool IsPrimaryImage { get; set; } = false;

		public string? RoomType { get; set; } // Living Room, Bedroom, Bathroom, etc.

		// Foreign Key
		public int HomestayId { get; set; }

		// Navigation Property
		public virtual Homestay Homestay { get; set; } = null!;
	}
}
