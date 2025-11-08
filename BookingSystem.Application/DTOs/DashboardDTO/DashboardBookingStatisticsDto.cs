using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.DashboardDTO
{
	public class DashboardBookingStatisticsDto
	{
		public int TotalBookings { get; set; }
		public int CompletedBookings { get; set; }
		public int PendingBookings { get; set; }
		public int CancelledBookings { get; set; }
		public decimal OccupancyRate { get; set; }
		public int NewHomestaysThisMonth { get; set; }
		public BookingStatusBreakdownDto StatusBreakdown { get; set; } = new();
		public List<TopHomestayDto> TopHomestays { get; set; } = new();
		public List<MonthlyBookingDto> MonthlyBookings { get; set; } = new();
	}

	public class BookingStatusBreakdownDto
	{
		public int Pending { get; set; }
		public int Confirmed { get; set; }
		public int CheckedIn { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
		public int Rejected { get; set; }
	}

	public class TopHomestayDto
	{
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public int BookingCount { get; set; }
		public double AverageRating { get; set; }
		public decimal TotalRevenue { get; set; }
	}

	public class MonthlyBookingDto
	{
		public string Month { get; set; } = string.Empty;
		public int TotalBookings { get; set; }
		public int Completed { get; set; }
		public int Cancelled { get; set; }
	}
}
