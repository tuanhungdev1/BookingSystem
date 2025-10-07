using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class DateRangeAvailabilityDto
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public bool IsAvailable { get; set; }
		public int TotalDays { get; set; }
		public int AvailableDays { get; set; }
		public int BlockedDays { get; set; }
		public decimal TotalPrice { get; set; }
		public List<DateAvailabilityDto> DateDetails { get; set; } = new List<DateAvailabilityDto>();
	}
}
