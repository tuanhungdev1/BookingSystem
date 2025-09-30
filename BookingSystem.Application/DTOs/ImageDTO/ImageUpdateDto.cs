using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class ImageUpdateDto
	{
		public IFormFile? NewFile { get; set; } // File mới (nếu có)
		public string? Folder { get; set; }
		public Dictionary<string, string>? Tags { get; set; }
	}
}
