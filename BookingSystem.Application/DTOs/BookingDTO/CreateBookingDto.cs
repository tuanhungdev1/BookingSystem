using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class CreateBookingDto
	{
		[Required(ErrorMessage = "Homestay ID is required")]
		public int HomestayId { get; set; }

		[Required(ErrorMessage = "Check-in date is required")]
		public DateTime CheckInDate { get; set; }

		[Required(ErrorMessage = "Check-out date is required")]
		public DateTime CheckOutDate { get; set; }

		[Required(ErrorMessage = "Number of guests is required")]
		[Range(1, 50, ErrorMessage = "Number of guests must be between 1 and 50")]
		public int NumberOfGuests { get; set; }

		[Required(ErrorMessage = "Number of adults is required")]
		[Range(1, 50, ErrorMessage = "Number of adults must be between 1 and 50")]
		public int NumberOfAdults { get; set; }

		[Range(0, 50, ErrorMessage = "Number of children must be between 0 and 50")]
		public int NumberOfChildren { get; set; } = 0;

		[Range(0, 10, ErrorMessage = "Number of infants must be between 0 and 10")]
		public int NumberOfInfants { get; set; } = 0;

		[MaxLength(1000, ErrorMessage = "Special requests cannot exceed 1000 characters")]
		public string? SpecialRequests { get; set; }
	}
}
