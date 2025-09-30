using BookingSystem.Application.Models.Constants;
using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.DTOs.AmenityDTO
{
	public class UpdateAmenityDto
	{
		public string? AmenityName { get; set; }
		public string? AmenityDescription { get; set; }
		public string? IconUrl { get; set; }
		public string? Category { get; set; }
		public IFormFile? IconFile { get; set; }
		public ImageAction ImageAction { get; set; } = ImageAction.Keep;
		public bool? IsActive { get; set; }
		public int? DisplayOrder { get; set; }
	}
}
