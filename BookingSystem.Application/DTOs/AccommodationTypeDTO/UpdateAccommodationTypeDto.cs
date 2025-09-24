using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AccommodationTypeDTO
{
	public class UpdateAccommodationTypeDto
	{
		[Required(ErrorMessage = "Name is required")]
		[StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "Description is required")]
		[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
		public string Description { get; set; } = string.Empty;

		[StringLength(255, ErrorMessage = "Image URL cannot exceed 255 characters")]
		public IFormFile? Image { get; set; }
	}
}
