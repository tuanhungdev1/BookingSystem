using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class CompleteDashboardDto
	{
		public DashboardOverviewDto Overview { get; set; } = new();
		public DashboardUserStatisticsDto UserStatistics { get; set; } = new();
		public DashboardBookingStatisticsDto BookingStatistics { get; set; } = new();
		public RevenueStatisticsDto RevenueStatistics { get; set; } = new();
		public ReviewStatisticsDto ReviewStatistics { get; set; } = new();
		public DateTime GeneratedAt { get; set; }
	}
}
