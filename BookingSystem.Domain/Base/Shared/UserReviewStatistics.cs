using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Shared
{
	public class UserReviewStatistics
	{
		public int TotalReviewsWritten { get; set; }
		public int TotalReviewsReceived { get; set; }
		public double AverageRatingReceived { get; set; }
		public int RecommendedCount { get; set; }
		public double RecommendationPercentage { get; set; }
	}
}
