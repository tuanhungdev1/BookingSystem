// Domain/Repositories/IDashboardRepository.cs
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Repositories
{
	public interface IDashboardRepository
	{
		// User Statistics
		Task<int> GetTotalUsersCountAsync();
		Task<int> GetActiveUsersCountAsync(int days);
		Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate);
		Task<Dictionary<string, int>> GetUsersByRegionAsync();
		Task<List<DailyUserActivity>> GetDailyUserActivityAsync(int days);

		// Host Statistics
		Task<int> GetTotalHostsCountAsync();
		Task<int> GetActiveHostsCountAsync(int days);
		Task<int> GetNewHostsCountAsync(DateTime startDate, DateTime endDate);
		Task<List<MonthlyHostData>> GetMonthlyHostDataAsync(int months);

		// Homestay Statistics
		Task<int> GetTotalHomestaysCountAsync();
		Task<int> GetActiveHomestaysCountAsync();
		Task<int> GetNewHomestaysCountAsync(DateTime startDate, DateTime endDate);
		Task<List<MonthlyHomestayData>> GetMonthlyHomestayDataAsync(int months);

		// Booking Statistics
		Task<BookingStatusCount> GetBookingStatusCountAsync(DateTime? startDate = null);
		Task<int> GetTotalBookedDaysAsync(DateTime startDate, DateTime endDate);
		Task<List<TopHomestayData>> GetTopHomestaysByBookingAsync(int take);
		Task<List<MonthlyBookingData>> GetMonthlyBookingDataAsync(int months);

		// Revenue Statistics
		Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
		Task<RevenueBreakdown> GetRevenueBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null);
		Task<List<MonthlyRevenueData>> GetMonthlyRevenueDataAsync(int months);
		Task<List<PaymentMethodData>> GetPaymentMethodStatsAsync(DateTime startDate);
		Task<RefundData> GetRefundDataAsync();

		// Review Statistics
		Task<ReviewOverview> GetReviewOverviewAsync();
		Task<Dictionary<int, int>> GetRatingDistributionAsync();
		Task<List<TopHomestayData>> GetTopRatedHomestaysAsync(int take);
		Task<List<TopHomestayData>> GetLowRatedHomestaysAsync(int take);
		Task<List<RecentReviewData>> GetRecentReviewsAsync(int take);
	}

	// Host Dashboard Overview
	public class HostDashboardOverview
	{
		public int TotalHomestays { get; set; }
		public int ActiveHomestays { get; set; }
		public int TotalBookings { get; set; }
		public decimal TotalRevenue { get; set; }
		public double AverageRating { get; set; }
		public int TotalReviews { get; set; }
	}

	// Host Homestay Performance
	public class HostHomestayPerformance
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

	// Host Revenue Statistics
	public class HostRevenueStats
	{
		public decimal TotalRevenue { get; set; }
		public decimal BaseRevenue { get; set; }
		public decimal CleaningFees { get; set; }
		public decimal ServiceFees { get; set; }
		public decimal TaxAmount { get; set; }
		public int BookingCount { get; set; }
		public decimal AverageBookingValue { get; set; }
	}

	// Monthly Host Revenue
	public class MonthlyHostRevenue
	{
		public DateTime Month { get; set; }
		public decimal Revenue { get; set; }
		public int BookingCount { get; set; }
		public int GuestCount { get; set; }
	}

	// Host Booking Statistics
	public class HostBookingStats
	{
		public int TotalBookings { get; set; }
		public int Pending { get; set; }
		public int Confirmed { get; set; }
		public int CheckedIn { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
		public int Rejected { get; set; }
		public decimal CancellationRate { get; set; }
	}

	// Host Booking Trend
	public class HostBookingTrend
	{
		public DateTime Month { get; set; }
		public int TotalBookings { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
		public int Pending { get; set; }
	}

	// Host Review Summary
	public class HostReviewSummary
	{
		public int ReviewId { get; set; }
		public string GuestName { get; set; } = string.Empty;
		public string HomestayTitle { get; set; } = string.Empty;
		public int OverallRating { get; set; }
		public int CleanlinessRating { get; set; }
		public int AccuracyRating { get; set; }
		public int CommunicationRating { get; set; }
		public int LocationRating { get; set; }
		public int ValueRating { get; set; }
		public string? Comment { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	// Host Occupancy Data
	public class HostOccupancyData
	{
		public int TotalDays { get; set; }
		public int BookedDays { get; set; }
		public decimal OccupancyRate { get; set; }
		public int HomestayCount { get; set; }
	}

	// Top Guest Data
	public class TopGuestData
	{
		public int GuestId { get; set; }
		public string GuestName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public int TotalBookings { get; set; }
		public decimal TotalSpent { get; set; }
		public DateTime LastBookingDate { get; set; }
	}

	// Calendar Booking (nested in HostCalendarData)
	public class CalendarBooking
	{
		public int BookingId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string GuestName { get; set; } = string.Empty;
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public string Status { get; set; } = string.Empty;
		public decimal TotalAmount { get; set; }
	}

	// Host Calendar Data
	public class HostCalendarData
	{
		public List<CalendarBooking> Bookings { get; set; } = new();
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	// DTOs for repository responses
	public class DailyUserActivity
	{
		public DateTime Date { get; set; }
		public int ActiveUsers { get; set; }
		public int NewUsers { get; set; }
	}

	public class MonthlyHostData
	{
		public DateTime Month { get; set; }
		public int Count { get; set; }
	}

	public class MonthlyHomestayData
	{
		public DateTime Month { get; set; }
		public int Count { get; set; }
	}

	public class BookingStatusCount
	{
		public int Total { get; set; }
		public int Pending { get; set; }
		public int Confirmed { get; set; }
		public int CheckedIn { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
		public int Rejected { get; set; }
	}

	public class TopHomestayData
	{
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string HostName { get; set; } = string.Empty;
		public int BookingCount { get; set; }
		public double AverageRating { get; set; }
		public decimal TotalRevenue { get; set; }
		public int ReviewCount { get; set; }
	}

	public class MonthlyBookingData
	{
		public DateTime Month { get; set; }
		public int TotalBookings { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
	}

	public class RevenueBreakdown
	{
		public decimal BaseAmount { get; set; }
		public decimal ServiceFee { get; set; }
		public decimal CleaningFee { get; set; }
		public decimal TaxAmount { get; set; }
	}

	public class MonthlyRevenueData
	{
		public DateTime Month { get; set; }
		public decimal Revenue { get; set; }
		public int BookingCount { get; set; }
	}

	public class PaymentMethodData
	{
		public PaymentMethod PaymentMethod { get; set; }
		public decimal TotalAmount { get; set; }
		public int Count { get; set; }
	}

	public class RefundData
	{
		public decimal TotalRefundAmount { get; set; }
		public int RefundCount { get; set; }
	}

	public class ReviewOverview
	{
		public double AverageRating { get; set; }
		public int TotalReviews { get; set; }
		public int NewReviewsThisMonth { get; set; }
		public int ComplaintCount { get; set; }
	}

	public class RecentReviewData
	{
		public int ReviewId { get; set; }
		public string GuestName { get; set; } = string.Empty;
		public string HomestayTitle { get; set; } = string.Empty;
		public int Rating { get; set; }
		public string? Comment { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}