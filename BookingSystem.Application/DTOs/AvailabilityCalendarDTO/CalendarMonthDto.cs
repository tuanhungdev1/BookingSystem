using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class CalendarMonthDto
	{
		public int Year { get; set; }
		public int Month { get; set; }
		public List<AvailabilityCalendarDto> Dates { get; set; } = new List<AvailabilityCalendarDto>();
	}
}
