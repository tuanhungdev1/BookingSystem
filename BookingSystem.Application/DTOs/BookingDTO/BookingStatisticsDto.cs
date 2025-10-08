namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class BookingStatisticsDto
	{
		public int TotalBookings { get; set; }
		public int PendingBookings { get; set; }
		public int ConfirmedBookings { get; set; }
		public int CompletedBookings { get; set; }
		public int CancelledBookings { get; set; }
		public decimal TotalRevenue { get; set; }
		public decimal AverageBookingValue { get; set; }
		public double OccupancyRate { get; set; }
	}
}
