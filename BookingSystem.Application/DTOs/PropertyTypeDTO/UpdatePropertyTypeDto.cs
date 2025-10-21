using BookingSystem.Application.Models.Constants;
using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.DTOs.PropertyTypeDTO
{
	public class UpdatePropertyTypeDto
	{
		public string? TypeName { get; set; }

		public string? Description { get; set; }

		public IFormFile? IconFile { get; set; }

		public ImageAction ImageAction { get; set; } = ImageAction.Keep;

		public bool? IsActive { get; set; }

		public int? DisplayOrder { get; set; }
	}
}
