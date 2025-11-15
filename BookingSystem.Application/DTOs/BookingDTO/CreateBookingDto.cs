using BookingSystem.Application.Attributes;
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


		[Required(ErrorMessage = "Guest full name is required")]
		[MaxLength(200)]
		public string GuestFullName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Guest email is required")]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		[MaxLength(200)]
		public string GuestEmail { get; set; } = string.Empty;

		[Required(ErrorMessage = "Guest phone number is required")]
		[Phone(ErrorMessage = "Invalid phone number format")]
		[MaxLength(20)]
		public string GuestPhoneNumber { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? GuestAddress { get; set; }

		[MaxLength(100)]
		public string? GuestCity { get; set; }

		[MaxLength(100)]
		public string? GuestCountry { get; set; }

		// ✅ THÊM: Thông tin đặt cho người khác (optional)
		public bool IsBookingForSomeoneElse { get; set; } = false;

		[MaxLength(200)]
		public string? ActualGuestFullName { get; set; }

		[ConditionalEmail(ErrorMessage = "Invalid actual guest email format")]
		[MaxLength(200)]
		public string? ActualGuestEmail { get; set; }

		[ConditionalPhone(ErrorMessage = "Invalid actual guest phone number format")]
		[MaxLength(20)]
		public string? ActualGuestPhoneNumber { get; set; }

		[MaxLength(50)]
		public string? ActualGuestIdNumber { get; set; }

		[MaxLength(1000)]
		public string? ActualGuestNotes { get; set; }
	}
}
