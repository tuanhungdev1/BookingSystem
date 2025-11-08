using BookingSystem.Application.DTOs.DashboardDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Contracts
{
	public interface IDashboardService
	{
		Task<DashboardOverviewDto> GetOverviewAsync(int months = 12);
		Task<DashboardUserStatisticsDto> GetUserStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
		Task<DashboardBookingStatisticsDto> GetBookingStatisticsAsync(int months = 12);
		Task<RevenueStatisticsDto> GetRevenueStatisticsAsync(int months = 12);
		Task<ReviewStatisticsDto> GetReviewStatisticsAsync(int months = 6);
	}
}
