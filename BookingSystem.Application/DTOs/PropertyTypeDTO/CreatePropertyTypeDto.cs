using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.PropertyTypeDTO
{
	public class CreatePropertyTypeDto
	{
		[Required(ErrorMessage = "TypeName is required")]
		[MaxLength(100, ErrorMessage = "TypeName cannot exceed 100 characters")]
		[MinLength(3, ErrorMessage = "TypeName must be at least 3 characters long")]
		public string TypeName { get; set; } = string.Empty;

		[MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
		public string? Description { get; set; }

		public IFormFile? IconFile { get; set; }

		public bool IsActive { get; set; } = true;

		[Range(0, 1000, ErrorMessage = "DisplayOrder must be between 0 and 1000")]
		public int DisplayOrder { get; set; } = 0;
	}
}
