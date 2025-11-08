using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class DashboardUserStatisticsDto
	{
		public int DailyActiveUsers { get; set; }
		public int MonthlyActiveUsers { get; set; }
		public int ActiveHosts { get; set; }
		public decimal UserToHostConversionRate { get; set; }
		public List<UserByRegionDto> UsersByRegion { get; set; } = new();
		public List<DailyActivityDto> DailyActivity { get; set; } = new();
		public List<DailyActivityDto> MonthlyActivity { get; set; } = new();
	}

	public class UserByRegionDto
	{
		public string Region { get; set; } = string.Empty;
		public int UserCount { get; set; }
		public int HostCount { get; set; }
		public decimal Percentage { get; set; }
	}

	public class DailyActivityDto
	{
		public DateTime Date { get; set; }
		public int ActiveUsers { get; set; }
		public int NewUsers { get; set; }
	}
}
