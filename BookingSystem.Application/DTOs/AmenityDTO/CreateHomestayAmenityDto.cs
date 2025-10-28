using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AmenityDTO
{
	public class CreateHomestayAmenityDto
	{
		[Required(ErrorMessage = "Amenity ID is required.")]
		public int AmenityId { get; set; }

		[MaxLength(500)]
		public string? CustomNote { get; set; }
	}
}
