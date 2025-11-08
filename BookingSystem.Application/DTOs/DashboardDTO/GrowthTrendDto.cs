using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class GrowthTrendDto
	{
		public int CurrentMonth { get; set; }
		public int PreviousMonth { get; set; }
		public decimal GrowthPercentage { get; set; }
		public List<MonthlyDataPoint> MonthlyData { get; set; } = new();
	}
}
