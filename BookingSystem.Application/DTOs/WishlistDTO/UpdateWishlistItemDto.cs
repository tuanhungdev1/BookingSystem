using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.WishlistDTO
{
	public class UpdateWishlistItemDto
	{
		[MaxLength(500, ErrorMessage = "Personal note cannot exceed 500 characters")]
		public string? PersonalNote { get; set; }
	}
}
