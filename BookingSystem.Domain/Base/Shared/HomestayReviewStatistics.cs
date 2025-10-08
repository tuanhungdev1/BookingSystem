using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Shared
{
	public class HomestayReviewStatistics
	{
		public int TotalReviews { get; set; }
		public double AverageOverallRating { get; set; }
		public double AverageCleanlinessRating { get; set; }
		public double AverageLocationRating { get; set; }
		public double AverageValueForMoneyRating { get; set; }
		public double AverageCommunicationRating { get; set; }
		public double AverageCheckInRating { get; set; }
		public int RecommendedCount { get; set; }
		public double RecommendationPercentage { get; set; }
		public Dictionary<int, int> RatingDistribution { get; set; } = new();
	}
}
