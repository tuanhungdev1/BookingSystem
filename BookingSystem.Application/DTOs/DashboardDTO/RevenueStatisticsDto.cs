using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class RevenueStatisticsDto
	{
		public decimal TotalRevenue { get; set; }
		public decimal MonthlyRevenue { get; set; }
		public decimal YearlyRevenue { get; set; }
		public decimal AverageRevenuePerBooking { get; set; }
		public decimal RefundAmount { get; set; }
		public decimal RefundRate { get; set; }
		public List<MonthlyRevenueDto> MonthlyRevenueData { get; set; } = new();
		public RevenueBreakdownDto RevenueBreakdown { get; set; } = new();
		public List<PaymentMethodStatsDto> PaymentMethodStats { get; set; } = new();
	}

	public class MonthlyRevenueDto
	{
		public string Month { get; set; } = string.Empty;
		public decimal Revenue { get; set; }
		public int BookingCount { get; set; }
		public decimal AveragePerBooking { get; set; }
	}

	public class RevenueBreakdownDto
	{
		public decimal BaseAmount { get; set; }
		public decimal ServiceFee { get; set; }
		public decimal CleaningFee { get; set; }
		public decimal TaxAmount { get; set; }
	}

	public class PaymentMethodStatsDto
	{
		public string PaymentMethod { get; set; } = string.Empty;
		public decimal TotalAmount { get; set; }
		public int Count { get; set; }
		public decimal Percentage { get; set; }
	}
}
