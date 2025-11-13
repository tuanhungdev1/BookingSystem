using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
	public class HostDashboardRepository : IHostDashboardRepository
	{
		private readonly BookingDbContext _context;

		public HostDashboardRepository(BookingDbContext context)
		{
			_context = context;
		}

		#region Host Dashboard Statistics

		public async Task<HostDashboardOverview> GetHostOverviewAsync(int hostId)
		{
			var homestays = await _context.Homestays
				.Where(h => h.OwnerId == hostId)
				.ToListAsync();

			var totalHomestays = homestays.Count;
			var activeHomestays = homestays.Count(h => h.IsActive && h.IsApproved);

			var totalBookings = await _context.Bookings
				.CountAsync(b => b.Homestay.OwnerId == hostId);

			var totalRevenue = await _context.Bookings
				.Where(b => b.Homestay.OwnerId == hostId &&
							b.BookingStatus == BookingStatus.Completed)
				.SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

			var homestaysWithReviews = homestays.Where(h => h.TotalReviews > 0).ToList();
			var avgRating = homestaysWithReviews.Any()
				? homestaysWithReviews.Average(h => h.RatingAverage)
				: 0;

			var totalReviews = homestays.Sum(h => h.TotalReviews);

			return new HostDashboardOverview
			{
				TotalHomestays = totalHomestays,
				ActiveHomestays = activeHomestays,
				TotalBookings = totalBookings,
				TotalRevenue = totalRevenue,
				AverageRating = avgRating,
				TotalReviews = totalReviews
			};
		}

		public async Task<List<HostHomestayPerformance>> GetHostHomestaysPerformanceAsync(int hostId)
		{
			return await _context.Homestays
				.Where(h => h.OwnerId == hostId)
				.Select(h => new HostHomestayPerformance
				{
					HomestayId = h.Id,
					HomestayTitle = h.HomestayTitle,
					BookingCount = h.BookingCount,
					Revenue = h.Bookings
						.Where(b => b.BookingStatus == BookingStatus.Completed)
						.Sum(b => (decimal?)b.TotalAmount) ?? 0,
					AverageRating = h.RatingAverage,
					ReviewCount = h.TotalReviews,
					OccupancyRate = 0, // Calculate separately if needed
					ViewCount = h.ViewCount
				})
				.OrderByDescending(h => h.Revenue)
				.ToListAsync();
		}

		public async Task<HostRevenueStats> GetHostRevenueStatsAsync(
			int hostId,
			DateTime startDate,
			DateTime endDate)
		{
			var bookings = await _context.Bookings
				.Where(b => b.Homestay.OwnerId == hostId &&
							b.BookingStatus == BookingStatus.Completed &&
							b.CreatedAt >= startDate &&
							b.CreatedAt < endDate)
				.ToListAsync();

			return new HostRevenueStats
			{
				TotalRevenue = bookings.Sum(b => b.TotalAmount),
				BaseRevenue = bookings.Sum(b => b.BaseAmount),
				CleaningFees = bookings.Sum(b => b.CleaningFee),
				ServiceFees = bookings.Sum(b => b.ServiceFee),
				TaxAmount = bookings.Sum(b => b.TaxAmount),
				BookingCount = bookings.Count,
				AverageBookingValue = bookings.Any()
					? bookings.Average(b => b.TotalAmount)
					: 0
			};
		}

		public async Task<List<MonthlyHostRevenue>> GetHostMonthlyRevenueAsync(
			int hostId,
			int months)
		{
			var result = new List<MonthlyHostRevenue>();

			for (int i = months - 1; i >= 0; i--)
			{
				var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
				monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var monthBookings = await _context.Bookings
					.Where(b => b.Homestay.OwnerId == hostId &&
								b.BookingStatus == BookingStatus.Completed &&
								b.CreatedAt >= monthStart &&
								b.CreatedAt < monthEnd)
					.ToListAsync();

				result.Add(new MonthlyHostRevenue
				{
					Month = monthStart,
					Revenue = monthBookings.Sum(b => b.TotalAmount),
					BookingCount = monthBookings.Count,
					GuestCount = monthBookings.Select(b => b.GuestId).Distinct().Count()
				});
			}

			return result;
		}

		public async Task<HostBookingStats> GetHostBookingStatsAsync(
			int hostId,
			DateTime startDate,
			DateTime endDate)
		{
			var bookings = await _context.Bookings
				.Where(b => b.Homestay.OwnerId == hostId &&
							b.CreatedAt >= startDate &&
							b.CreatedAt < endDate)
				.ToListAsync();

			var totalBookings = bookings.Count;
			var cancelledCount = bookings.Count(b => b.BookingStatus == BookingStatus.Cancelled);

			return new HostBookingStats
			{
				TotalBookings = totalBookings,
				Pending = bookings.Count(b => b.BookingStatus == BookingStatus.Pending),
				Confirmed = bookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
				CheckedIn = bookings.Count(b => b.BookingStatus == BookingStatus.CheckedIn),
				Completed = bookings.Count(b => b.BookingStatus == BookingStatus.Completed),
				Cancelled = cancelledCount,
				Rejected = bookings.Count(b => b.BookingStatus == BookingStatus.Rejected),
				CancellationRate = totalBookings > 0
					? (decimal)cancelledCount / totalBookings * 100
					: 0
			};
		}

		public async Task<List<HostBookingTrend>> GetHostBookingTrendsAsync(
			int hostId,
			int months)
		{
			var result = new List<HostBookingTrend>();

			for (int i = months - 1; i >= 0; i--)
			{
				var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
				monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var monthBookings = await _context.Bookings
					.Where(b => b.Homestay.OwnerId == hostId &&
								b.CreatedAt >= monthStart &&
								b.CreatedAt < monthEnd)
					.ToListAsync();

				result.Add(new HostBookingTrend
				{
					Month = monthStart,
					TotalBookings = monthBookings.Count,
					Completed = monthBookings.Count(b => b.BookingStatus == BookingStatus.Completed),
					Cancelled = monthBookings.Count(b => b.BookingStatus == BookingStatus.Cancelled),
					Pending = monthBookings.Count(b => b.BookingStatus == BookingStatus.Pending)
				});
			}

			return result;
		}

		public async Task<List<HostReviewSummary>> GetHostReviewsAsync(
			int hostId,
			int take)
		{
			return await _context.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.Homestay)
				.Where(r => r.Homestay.OwnerId == hostId && r.IsVisible)
				.OrderByDescending(r => r.CreatedAt)
				.Take(take)
				.Select(r => new HostReviewSummary
				{
					ReviewId = r.Id,
					GuestName = r.Reviewer.FullName,
					HomestayTitle = r.Homestay.HomestayTitle,
					OverallRating = r.OverallRating,
					CleanlinessRating = r.CleanlinessRating,
					AccuracyRating = 0, // Không có trong entity Review hiện tại
					CommunicationRating = r.CommunicationRating,
					LocationRating = r.LocationRating,
					ValueRating = r.ValueForMoneyRating,
					Comment = r.ReviewComment,
					CreatedAt = r.CreatedAt
				})
				.ToListAsync();
		}

		public async Task<HostOccupancyData> GetHostOccupancyRateAsync(
			int hostId,
			DateTime startDate,
			DateTime endDate)
		{
			var homestays = await _context.Homestays
				.Where(h => h.OwnerId == hostId && h.IsActive)
				.ToListAsync();

			var totalDays = (endDate - startDate).Days;
			var totalAvailableDays = homestays.Count * totalDays;

			var bookedDays = await _context.Bookings
				.Where(b => b.Homestay.OwnerId == hostId &&
							(b.BookingStatus == BookingStatus.Completed ||
							 b.BookingStatus == BookingStatus.Confirmed ||
							 b.BookingStatus == BookingStatus.CheckedIn) &&
							b.CheckInDate >= startDate &&
							b.CheckOutDate <= endDate)
				.SumAsync(b => (int?)EF.Functions.DateDiffDay(b.CheckInDate, b.CheckOutDate)) ?? 0;

			return new HostOccupancyData
			{
				TotalDays = totalAvailableDays,
				BookedDays = bookedDays,
				OccupancyRate = totalAvailableDays > 0
					? (decimal)bookedDays / totalAvailableDays * 100
					: 0,
				HomestayCount = homestays.Count
			};
		}

		public async Task<List<TopGuestData>> GetHostTopGuestsAsync(
			int hostId,
			int take)
		{
			return await _context.Bookings
				.Where(b => b.Homestay.OwnerId == hostId &&
							b.BookingStatus == BookingStatus.Completed)
				.GroupBy(b => new
				{
					b.GuestId,
					b.Guest.FirstName,
					b.Guest.LastName,
					b.Guest.Email
				})
				.Select(g => new TopGuestData
				{
					GuestId = g.Key.GuestId,
					GuestName = g.Key.FirstName + " " + g.Key.LastName,
					Email = g.Key.Email ?? string.Empty,
					TotalBookings = g.Count(),
					TotalSpent = g.Sum(b => b.TotalAmount),
					LastBookingDate = g.Max(b => b.CreatedAt)
				})
				.OrderByDescending(g => g.TotalBookings)
				.ThenByDescending(g => g.TotalSpent)
				.Take(take)
				.ToListAsync();
		}

		public async Task<HostCalendarData> GetHostCalendarDataAsync(
			int hostId,
			DateTime startDate,
			DateTime endDate)
		{
			var bookings = await _context.Bookings
				.Include(b => b.Homestay)
				.Include(b => b.Guest)
				.Where(b => b.Homestay.OwnerId == hostId &&
							b.CheckInDate <= endDate &&
							b.CheckOutDate >= startDate &&
							(b.BookingStatus == BookingStatus.Confirmed ||
							 b.BookingStatus == BookingStatus.CheckedIn ||
							 b.BookingStatus == BookingStatus.Pending))
				.Select(b => new CalendarBooking
				{
					BookingId = b.Id,
					HomestayTitle = b.Homestay.HomestayTitle,
					GuestName = b.Guest.FirstName + " " + b.Guest.LastName,
					CheckInDate = b.CheckInDate,
					CheckOutDate = b.CheckOutDate,
					Status = b.BookingStatus.ToString(),
					TotalAmount = b.TotalAmount
				})
				.ToListAsync();

			return new HostCalendarData
			{
				Bookings = bookings,
				StartDate = startDate,
				EndDate = endDate
			};
		}

		#endregion
	}
}