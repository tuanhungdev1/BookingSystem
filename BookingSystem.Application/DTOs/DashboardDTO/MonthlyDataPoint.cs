using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class MonthlyDataPoint
	{
		public string Month { get; set; } = string.Empty;
		public int Value { get; set; }
	}
}
