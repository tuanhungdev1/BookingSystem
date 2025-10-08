using BookingSystem.Application.DTOs.AccommodationDTO;

namespace BookingSystem.Application.DTOs.WishlistDTO
{
	public class WishlistItemDto
	{
		public int Id { get; set; }
		public string? PersonalNote { get; set; }
		public int UserId { get; set; }
		public int HomestayId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		// Homestay details
		public HomestayDto Homestay { get; set; } = null!;
	}
}
