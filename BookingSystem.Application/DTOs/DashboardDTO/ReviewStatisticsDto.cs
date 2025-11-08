using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class ReviewStatisticsDto
	{
		public double AverageRating { get; set; }
		public int TotalReviews { get; set; }
		public int NewReviewsThisMonth { get; set; }
		public int ComplaintCount { get; set; }
		public RatingDistributionDto RatingDistribution { get; set; } = new();
		public List<TopRatedHomestayDto> TopRatedHomestays { get; set; } = new();
		public List<LowRatedHomestayDto> LowRatedHomestays { get; set; } = new();
		public List<RecentReviewDto> RecentReviews { get; set; } = new();
	}

	public class RatingDistributionDto
	{
		public int FiveStar { get; set; }
		public int FourStar { get; set; }
		public int ThreeStar { get; set; }
		public int TwoStar { get; set; }
		public int OneStar { get; set; }
	}

	public class TopRatedHomestayDto
	{
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string HostName { get; set; } = string.Empty;
		public double AverageRating { get; set; }
		public int ReviewCount { get; set; }
	}

	public class LowRatedHomestayDto
	{
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string HostName { get; set; } = string.Empty;
		public double AverageRating { get; set; }
		public int ReviewCount { get; set; }
		public string? MostCommonComplaint { get; set; }
	}

	public class RecentReviewDto
	{
		public int ReviewId { get; set; }
		public string GuestName { get; set; } = string.Empty;
		public string HomestayTitle { get; set; } = string.Empty;
		public int Rating { get; set; }
		public string? Comment { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
