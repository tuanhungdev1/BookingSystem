using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class ImageReplaceDto
	{
		public string OldPublicId { get; set; } = string.Empty;
		public IFormFile NewFile { get; set; } = null!;
		public string? Folder { get; set; }
	}
}
