using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class BlockDatesDto
	{
		[Required(ErrorMessage = "Start date is required")]
		public DateOnly StartDate { get; set; }

		[Required(ErrorMessage = "End date is required")]
		public DateOnly EndDate { get; set; }

		[Required(ErrorMessage = "Block reason is required")]
		[MaxLength(500, ErrorMessage = "Block reason cannot exceed 500 characters")]
		public string BlockReason { get; set; } = string.Empty;
	}
}
