using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class UpdateBookingDto
	{
		public DateTime? CheckInDate { get; set; }
		public DateTime? CheckOutDate { get; set; }

		[Range(1, 50, ErrorMessage = "Number of guests must be between 1 and 50")]
		public int? NumberOfGuests { get; set; }

		[Range(1, 50, ErrorMessage = "Number of adults must be between 1 and 50")]
		public int? NumberOfAdults { get; set; }

		[Range(0, 50, ErrorMessage = "Number of children must be between 0 and 50")]
		public int? NumberOfChildren { get; set; }

		[Range(0, 10, ErrorMessage = "Number of infants must be between 0 and 10")]
		public int? NumberOfInfants { get; set; }

		[MaxLength(1000, ErrorMessage = "Special requests cannot exceed 1000 characters")]
		public string? SpecialRequests { get; set; }
	}
}
