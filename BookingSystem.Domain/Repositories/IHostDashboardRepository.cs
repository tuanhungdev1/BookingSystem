using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Repositories
{
    public interface IHostDashboardRepository
    {
		// Host-specific Statistics
		Task<HostDashboardOverview> GetHostOverviewAsync(int hostId);
		Task<List<HostHomestayPerformance>> GetHostHomestaysPerformanceAsync(int hostId);
		Task<HostRevenueStats> GetHostRevenueStatsAsync(int hostId, DateTime startDate, DateTime endDate);
		Task<List<MonthlyHostRevenue>> GetHostMonthlyRevenueAsync(int hostId, int months);
		Task<HostBookingStats> GetHostBookingStatsAsync(int hostId, DateTime startDate, DateTime endDate);
		Task<List<HostBookingTrend>> GetHostBookingTrendsAsync(int hostId, int months);
		Task<List<HostReviewSummary>> GetHostReviewsAsync(int hostId, int take);
		Task<HostOccupancyData> GetHostOccupancyRateAsync(int hostId, DateTime startDate, DateTime endDate);
		Task<List<TopGuestData>> GetHostTopGuestsAsync(int hostId, int take);
		Task<HostCalendarData> GetHostCalendarDataAsync(int hostId, DateTime startDate, DateTime endDate);
	}
}
