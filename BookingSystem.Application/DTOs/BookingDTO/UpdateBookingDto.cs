using BookingSystem.Application.Attributes;
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

		// ✅ THÊM: Thông tin người đặt (chỉ Admin mới được update)
		[MaxLength(200)]
		public string? GuestFullName { get; set; }

		[ConditionalEmail(ErrorMessage = "Invalid actual guest email format")]
		[MaxLength(200)]
		public string? GuestEmail { get; set; }

		[ConditionalPhone(ErrorMessage = "Invalid actual guest phone number format")]
		[MaxLength(20)]
		public string? GuestPhoneNumber { get; set; }

		[MaxLength(500)]
		public string? GuestAddress { get; set; }

		[MaxLength(100)]
		public string? GuestCity { get; set; }

		[MaxLength(100)]
		public string? GuestCountry { get; set; }

		// ✅ THÊM: Thông tin người ở thực tế (Guest có thể update)
		public bool? IsBookingForSomeoneElse { get; set; }

		[MaxLength(200)]
		public string? ActualGuestFullName { get; set; }

		[EmailAddress(ErrorMessage = "Invalid actual guest email format")]
		[MaxLength(200)]
		public string? ActualGuestEmail { get; set; }

		[Phone(ErrorMessage = "Invalid actual guest phone number format")]
		[MaxLength(20)]
		public string? ActualGuestPhoneNumber { get; set; }

		[MaxLength(50)]
		public string? ActualGuestIdNumber { get; set; }

		[MaxLength(1000)]
		public string? ActualGuestNotes { get; set; }
	}
}
