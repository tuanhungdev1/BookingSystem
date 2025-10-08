namespace BookingSystem.Application.DTOs.ReviewDTO
{
	public class ReviewDto
	{
		public int Id { get; set; }
		public int OverallRating { get; set; }
		public int CleanlinessRating { get; set; }
		public int LocationRating { get; set; }
		public int ValueForMoneyRating { get; set; }
		public int CommunicationRating { get; set; }
		public int CheckInRating { get; set; }
		public string? ReviewComment { get; set; }
		public bool IsVisible { get; set; }
		public bool IsRecommended { get; set; }
		public int HelpfulCount { get; set; }
		public string? HostResponse { get; set; }
		public DateTime? HostRespondedAt { get; set; }
		public int ReviewerId { get; set; }
		public string ReviewerName { get; set; } = string.Empty;
		public string? ReviewerAvatar { get; set; }
		public int RevieweeId { get; set; }
		public string RevieweeName { get; set; } = string.Empty;
		public int BookingId { get; set; }
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

}
