using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class CompleteHostDashboardDto
	{
		public HostDashboardOverviewDto Overview { get; set; } = new();
		public HostRevenueStatisticsDto Revenue { get; set; } = new();
		public HostBookingStatisticsDto Bookings { get; set; } = new();
		public HostReviewStatisticsDto Reviews { get; set; } = new();
		public HostPerformanceDto Performance { get; set; } = new();
		public DateTime GeneratedAt { get; set; }
	}
}
