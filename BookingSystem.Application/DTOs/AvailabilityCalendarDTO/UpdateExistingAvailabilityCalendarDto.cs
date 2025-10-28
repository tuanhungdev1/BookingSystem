using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class UpdateExistingAvailabilityCalendarDto
	{
		[Required]
		public int CalendarId { get; set; }

		public DateOnly? AvailableDate { get; set; }

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
