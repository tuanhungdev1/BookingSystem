using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class CreateAvailabilityCalendarDto
	{
		[Required(ErrorMessage = "Homestay ID is required")]
		public int HomestayId { get; set; }

		[Required(ErrorMessage = "Available date is required")]
		public DateOnly AvailableDate { get; set; }

		public bool IsAvailable { get; set; } = true;

		[Range(0, double.MaxValue, ErrorMessage = "Custom price must be non-negative")]
		public decimal? CustomPrice { get; set; }

		[Range(1, 365, ErrorMessage = "Minimum nights must be between 1 and 365")]
		public int? MinimumNights { get; set; }

		public bool IsBlocked { get; set; } = false;

		[MaxLength(500, ErrorMessage = "Block reason cannot exceed 500 characters")]
		public string? BlockReason { get; set; }
	}
}
