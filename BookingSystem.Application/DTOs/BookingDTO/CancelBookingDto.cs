using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class CancelBookingDto
	{
		[Required(ErrorMessage = "Cancellation reason is required")]
		[MaxLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters")]
		public string CancellationReason { get; set; } = string.Empty;
	}
}
