using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.HomestayImageDTO
{
	public class CreateHomestayImageDto
	{
		[Required(ErrorMessage = "Image file is required.")]
		public IFormFile ImageFile { get; set; } = null!;

		[MaxLength(200)]
		public string? ImageTitle { get; set; }

		[MaxLength(500)]
		public string? ImageDescription { get; set; }

		[Range(0, 100)]
		public int DisplayOrder { get; set; } = 0;

		public bool IsPrimaryImage { get; set; } = false;

		[MaxLength(50)]
		public string? RoomType { get; set; }
	}
}
