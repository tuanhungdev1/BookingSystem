namespace BookingSystem.Application.DTOs.AvailabilityCalendarDTO
{
	public class AvailabilityCalendarDto
	{
		public int Id { get; set; }
		public DateOnly AvailableDate { get; set; }
		public bool IsAvailable { get; set; }
		public decimal? CustomPrice { get; set; }
		public int? MinimumNights { get; set; }
		public bool IsBlocked { get; set; }
		public string? BlockReason { get; set; }
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public decimal BaseNightlyPrice { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
