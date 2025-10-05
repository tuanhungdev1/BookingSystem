using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.HomestayAmenityDTO
{
	public class CreateHomestayAmenityDto
	{
		[Required(ErrorMessage = "Amenity ID is required.")]
		public int AmenityId { get; set; }

		// Cho phép thêm ghi chú tùy chỉnh
		[MaxLength(500)]
		public string? CustomNote { get; set; }

		// Đánh dấu là amenity nổi bật
		public bool IsHighlight { get; set; } = false;
	}
}
