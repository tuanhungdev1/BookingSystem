using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class UpdateAvailabilityCalendarDto
	{
		public bool? IsAvailable { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Custom price must be non-negative")]
		public decimal? CustomPrice { get; set; }

		[Range(1, 365, ErrorMessage = "Minimum nights must be between 1 and 365")]
		public int? MinimumNights { get; set; }

		public bool? IsBlocked { get; set; }

		[MaxLength(500, ErrorMessage = "Block reason cannot exceed 500 characters")]
		public string? BlockReason { get; set; }
	}
}
