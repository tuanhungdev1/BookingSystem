using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.ReviewDTO
{
	public class ReviewExportDto
	{
		public int Id { get; set; }
		public int BookingId { get; set; }
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public int ReviewerId { get; set; }
		public string ReviewerName { get; set; } = string.Empty;
		public int RevieweeId { get; set; }
		public string RevieweeName { get; set; } = string.Empty;
		public int OverallRating { get; set; }
		public int CleanlinessRating { get; set; }
		public int LocationRating { get; set; }
		public int ValueForMoneyRating { get; set; }
		public int CommunicationRating { get; set; }
		public int CheckInRating { get; set; }
		public string ReviewComment { get; set; } = string.Empty;
		public string HostResponse { get; set; } = string.Empty;
		public DateTime? HostRespondedAt { get; set; }
		public int HelpfulCount { get; set; }
		public bool IsRecommended { get; set; }
		public bool IsVisible { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
