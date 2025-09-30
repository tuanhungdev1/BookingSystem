using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class MultipleImageUploadDto
	{
		public List<IFormFile> Files { get; set; } = new();
		public string? Folder { get; set; }
	}
}
