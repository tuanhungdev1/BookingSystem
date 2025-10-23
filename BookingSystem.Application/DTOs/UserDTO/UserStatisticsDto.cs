namespace BookingSystem.Application.DTOs.UserDTO
{
	public class UserStatisticsDto
	{
		public int TotalActiveUsers { get; set; }
		public int NewUsersThisMonth { get; set; }
		public int NewUsersLastMonth { get; set; }
		public decimal PercentageChange { get; set; }
		public string TrendDirection { get; set; } // "up" or "down"
		public int TotalUsers { get; set; }
		public int DeletedUsers { get; set; }
	}
}
