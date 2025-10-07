using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class AvailabilityRangeRequest
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
	}
}
