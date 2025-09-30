using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class ImageUploadDto
	{
		public IFormFile File { get; set; } = null!;
		public string? Folder { get; set; }
	}
}
