using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	// Host Dashboard Overview DTO
	public class HostDashboardOverviewDto
	{
		public int TotalHomestays { get; set; }
		public int ActiveHomestays { get; set; }
		public int TotalBookings { get; set; }
		public decimal TotalRevenue { get; set; }
		public double AverageRating { get; set; }
		public int TotalReviews { get; set; }
		public decimal RevenueGrowth { get; set; }
		public decimal MonthlyRevenue { get; set; }
	}

	// Host Revenue Statistics DTO
	public class HostRevenueStatisticsDto
	{
		public decimal TotalRevenue { get; set; }
		public decimal MonthlyRevenue { get; set; }
		public decimal YearlyRevenue { get; set; }
		public decimal AverageBookingValue { get; set; }
		public HostRevenueBreakdownDto RevenueBreakdown { get; set; } = new();
		public List<MonthlyHostRevenueDto> MonthlyRevenueData { get; set; } = new();
	}

	// Host Revenue Breakdown DTO
	public class HostRevenueBreakdownDto
	{
		public decimal BaseRevenue { get; set; }
		public decimal CleaningFees { get; set; }
		public decimal ServiceFees { get; set; }
		public decimal TaxAmount { get; set; }
	}

	// Monthly Host Revenue DTO
	public class MonthlyHostRevenueDto
	{
		public string Month { get; set; } = string.Empty;
		public decimal Revenue { get; set; }
		public int BookingCount { get; set; }
		public int GuestCount { get; set; }
	}

	// Host Booking Statistics DTO
	public class HostBookingStatisticsDto
	{
		public int TotalBookings { get; set; }
		public int PendingBookings { get; set; }
		public int ConfirmedBookings { get; set; }
		public int CompletedBookings { get; set; }
		public int CancelledBookings { get; set; }
		public decimal CancellationRate { get; set; }
		public decimal OccupancyRate { get; set; }
		public HostBookingStatusDto StatusBreakdown { get; set; } = new();
		public List<MonthlyBookingTrendDto> MonthlyTrends { get; set; } = new();
	}

	// Host Booking Status DTO
	public class HostBookingStatusDto
	{
		public int Pending { get; set; }
		public int Confirmed { get; set; }
		public int CheckedIn { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
		public int Rejected { get; set; }
	}

	// Monthly Booking Trend DTO
	public class MonthlyBookingTrendDto
	{
		public string Month { get; set; } = string.Empty;
		public int TotalBookings { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
		public int Pending { get; set; }
	}

	// Host Review Statistics DTO
	public class HostReviewStatisticsDto
	{
		public double AverageRating { get; set; }
		public int TotalReviews { get; set; }
		public double AverageCleanlinessRating { get; set; }
		public double AverageAccuracyRating { get; set; }
		public double AverageCommunicationRating { get; set; }
		public double AverageLocationRating { get; set; }
		public double AverageValueRating { get; set; }
		public HostRatingDistributionDto RatingDistribution { get; set; } = new();
		public List<HostRecentReviewDto> RecentReviews { get; set; } = new();
	}

	// Host Rating Distribution DTO
	public class HostRatingDistributionDto
	{
		public int FiveStar { get; set; }
		public int FourStar { get; set; }
		public int ThreeStar { get; set; }
		public int TwoStar { get; set; }
		public int OneStar { get; set; }
	}

	// Host Recent Review DTO
	public class HostRecentReviewDto
	{
		public int ReviewId { get; set; }
		public string GuestName { get; set; } = string.Empty;
		public string HomestayTitle { get; set; } = string.Empty;
		public int Rating { get; set; }
		public string? Comment { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	// Host Performance DTO
	public class HostPerformanceDto
	{
		public List<HomestayPerformanceDto> HomestayPerformance { get; set; } = new();
		public List<TopGuestDto> TopGuests { get; set; } = new();
		public List<UpcomingBookingDto> UpcomingBookings { get; set; } = new();
	}

	// Homestay Performance DTO
	public class HomestayPerformanceDto
	{
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public int BookingCount { get; set; }
		public decimal Revenue { get; set; }
		public double AverageRating { get; set; }
		public int ReviewCount { get; set; }
		public decimal OccupancyRate { get; set; }
		public int ViewCount { get; set; }
	}

	// Top Guest DTO
	public class TopGuestDto
	{
		public int GuestId { get; set; }
		public string GuestName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public int TotalBookings { get; set; }
		public decimal TotalSpent { get; set; }
		public DateTime LastBookingDate { get; set; }
	}

	// Upcoming Booking DTO
	public class UpcomingBookingDto
	{
		public int BookingId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string GuestName { get; set; } = string.Empty;
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public string Status { get; set; } = string.Empty;
		public decimal TotalAmount { get; set; }
	}
}
