namespace BookingSystem.Application.DTOs.ReviewDTO
{
	public class CreateReviewDto
	{
		public int OverallRating { get; set; }
		public int CleanlinessRating { get; set; }
		public int LocationRating { get; set; }
		public int ValueForMoneyRating { get; set; }
		public int CommunicationRating { get; set; }
		public int CheckInRating { get; set; }
		public string? ReviewComment { get; set; }
		public bool IsRecommended { get; set; } = true;
		public int BookingId { get; set; }
	}
}
