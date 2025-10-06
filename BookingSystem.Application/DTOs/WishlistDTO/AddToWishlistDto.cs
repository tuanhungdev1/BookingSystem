using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.WishlistDTO
{
	public class AddToWishlistDto
	{
		[Required(ErrorMessage = "Homestay ID is required")]
		public int HomestayId { get; set; }

		[MaxLength(500, ErrorMessage = "Personal note cannot exceed 500 characters")]
		public string? PersonalNote { get; set; }
	}
}
