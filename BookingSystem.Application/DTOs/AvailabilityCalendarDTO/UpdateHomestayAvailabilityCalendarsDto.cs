using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class UpdateHomestayAvailabilityCalendarsDto
	{
		
		public List<CreateAvailabilityCalendarDto>? NewCalendars { get; set; }

		
		public List<UpdateExistingAvailabilityCalendarDto>? UpdateCalendars { get; set; }

		
		public List<int>? DeleteCalendarIds { get; set; }
	}
}
