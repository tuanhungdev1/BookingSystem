using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.HomestayAmenityDTO
{
	public class CreateHomestayAmenityDto
	{
		[Required(ErrorMessage = "Amenity ID is required.")]
		public int AmenityId { get; set; }
	}
}
