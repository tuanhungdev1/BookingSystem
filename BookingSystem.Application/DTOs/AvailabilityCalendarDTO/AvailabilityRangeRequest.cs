namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class AvailabilityRangeRequest
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
	}
}
