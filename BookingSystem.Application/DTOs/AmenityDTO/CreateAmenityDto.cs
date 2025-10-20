using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.AmenityDTO
{
	public class CreateAmenityDto
	{
		[Required(ErrorMessage = "Amenity name is required.")]
		[MaxLength(100, ErrorMessage = "Amenity name cannot exceed 100 characters.")]
		public string AmenityName { get; set; } = string.Empty;

		[MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
		public string? AmenityDescription { get; set; }

		[Required(ErrorMessage = "Category is required.")]
		[MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
		public string Category { get; set; } = string.Empty;
		public bool IsActive { get; set; } = true;

		[Range(0, int.MaxValue, ErrorMessage = "DisplayOrder must be greater than or equal to 0.")]
		public int DisplayOrder { get; set; } = 0;
		public IFormFile? IconFile { get; set; }
	}
}
