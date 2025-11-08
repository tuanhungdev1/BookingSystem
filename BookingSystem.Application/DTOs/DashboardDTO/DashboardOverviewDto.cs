using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class DashboardOverviewDto
	{
		public int TotalUsers { get; set; }
		public int TotalHosts { get; set; }
		public int TotalHomestays { get; set; }
		public int ActiveHomestays { get; set; }
		public decimal MonthlyGrowthRate { get; set; }
		public GrowthTrendDto UserGrowth { get; set; } = new();
		public GrowthTrendDto HostGrowth { get; set; } = new();
		public GrowthTrendDto HomestayGrowth { get; set; } = new();
	}
}
