namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class DateAvailabilityDto
	{
		public DateOnly Date { get; set; }
		public bool IsAvailable { get; set; }
		public bool IsBlocked { get; set; }
		public decimal Price { get; set; }
		public int? MinimumNights { get; set; }
		public string? BlockReason { get; set; }
	}
}
