// Infrastructure/Repositories/DashboardRepository.cs
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
	public class DashboardRepository : IDashboardRepository
	{
		private readonly BookingDbContext _context;

		public DashboardRepository(BookingDbContext context)
		{
			_context = context;
		}

		#region User Statistics

		public async Task<int> GetTotalUsersCountAsync()
		{
			return await _context.Users.CountAsync(u => !u.IsDeleted);
		}

		public async Task<int> GetActiveUsersCountAsync(int days)
		{
			var cutoffDate = DateTime.UtcNow.AddDays(-days);
			return await _context.Users
				.CountAsync(u => !u.IsDeleted && u.IsActive);
		}

		public async Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate)
		{
			return await _context.Users
				.CountAsync(u => !u.IsDeleted && u.CreatedAt >= startDate && u.CreatedAt < endDate);
		}

		public async Task<Dictionary<string, int>> GetUsersByRegionAsync()
		{
			return await _context.Users
				.Where(u => !u.IsDeleted && u.City != null)
				.GroupBy(u => u.City)
				.Select(g => new { City = g.Key, Count = g.Count() })
				.OrderByDescending(x => x.Count)
				.Take(10)
				.ToDictionaryAsync(x => x.City ?? "Unknown", x => x.Count);
		}

		public async Task<List<DailyUserActivity>> GetDailyUserActivityAsync(int days)
		{
			var result = new List<DailyUserActivity>();

			for (int i = days - 1; i >= 0; i--)
			{
				var date = DateTime.UtcNow.AddDays(-i).Date;
				var nextDay = date.AddDays(1);

				var activeUsers = await _context.Users
					.CountAsync(u => !u.IsDeleted && u.IsActive);

				var newUsers = await _context.Users
					.CountAsync(u => !u.IsDeleted && u.CreatedAt >= date && u.CreatedAt < nextDay);

				result.Add(new DailyUserActivity
				{
					Date = date,
					ActiveUsers = activeUsers,
					NewUsers = newUsers
				});
			}

			return result;
		}

		#endregion

		#region Host Statistics

		public async Task<int> GetTotalHostsCountAsync()
		{
			return await _context.HostProfiles
				.CountAsync(h => h.Status == HostStatus.Approved);
		}

		public async Task<int> GetActiveHostsCountAsync(int days)
		{
			var cutoffDate = DateTime.UtcNow.AddDays(-days);
			return await _context.HostProfiles
				.CountAsync(h => h.Status == HostStatus.Approved &&
								 h.IsActive);
		}

		public async Task<int> GetNewHostsCountAsync(DateTime startDate, DateTime endDate)
		{
			return await _context.HostProfiles
				.CountAsync(h => h.Status == HostStatus.Approved &&
								 h.RegisteredAsHostAt >= startDate &&
								 h.RegisteredAsHostAt < endDate);
		}

		public async Task<List<MonthlyHostData>> GetMonthlyHostDataAsync(int months)
		{
			var result = new List<MonthlyHostData>();

			for (int i = months - 1; i >= 0; i--)
			{
				var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
				monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var count = await _context.HostProfiles
					.CountAsync(h => h.Status == HostStatus.Approved &&
									 h.RegisteredAsHostAt >= monthStart &&
									 h.RegisteredAsHostAt < monthEnd);

				result.Add(new MonthlyHostData
				{
					Month = monthStart,
					Count = count
				});
			}

			return result;
		}

		#endregion

		#region Homestay Statistics

		public async Task<int> GetTotalHomestaysCountAsync()
		{
			return await _context.Homestays.CountAsync();
		}

		public async Task<int> GetActiveHomestaysCountAsync()
		{
			return await _context.Homestays
				.CountAsync(h => h.IsActive && h.IsApproved);
		}

		public async Task<int> GetNewHomestaysCountAsync(DateTime startDate, DateTime endDate)
		{
			return await _context.Homestays
				.CountAsync(h => h.CreatedAt >= startDate && h.CreatedAt < endDate);
		}

		public async Task<List<MonthlyHomestayData>> GetMonthlyHomestayDataAsync(int months)
		{
			var result = new List<MonthlyHomestayData>();

			for (int i = months - 1; i >= 0; i--)
			{
				var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
				monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var count = await _context.Homestays
					.CountAsync(h => h.CreatedAt >= monthStart && h.CreatedAt < monthEnd);

				result.Add(new MonthlyHomestayData
				{
					Month = monthStart,
					Count = count
				});
			}

			return result;
		}

		#endregion

		#region Booking Statistics

		public async Task<BookingStatusCount> GetBookingStatusCountAsync(DateTime? startDate = null)
		{
			var query = _context.Bookings.AsQueryable();

			if (startDate.HasValue)
			{
				query = query.Where(b => b.CreatedAt >= startDate.Value);
			}

			var bookings = await query.ToListAsync();

			return new BookingStatusCount
			{
				Total = bookings.Count,
				Pending = bookings.Count(b => b.BookingStatus == BookingStatus.Pending),
				Confirmed = bookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
				CheckedIn = bookings.Count(b => b.BookingStatus == BookingStatus.CheckedIn),
				Completed = bookings.Count(b => b.BookingStatus == BookingStatus.Completed),
				Cancelled = bookings.Count(b => b.BookingStatus == BookingStatus.Cancelled),
				Rejected = bookings.Count(b => b.BookingStatus == BookingStatus.Rejected)
			};
		}

		public async Task<int> GetTotalBookedDaysAsync(DateTime startDate, DateTime endDate)
		{
			return await _context.Bookings
				.Where(b => (b.BookingStatus == BookingStatus.Completed ||
							 b.BookingStatus == BookingStatus.Confirmed) &&
							b.CheckInDate >= startDate &&
							b.CheckOutDate <= endDate)
				.SumAsync(b => EF.Functions.DateDiffDay(b.CheckInDate, b.CheckOutDate));
		}

		public async Task<List<TopHomestayData>> GetTopHomestaysByBookingAsync(int take)
		{
			return await _context.Homestays
				.Include(h => h.Owner)
				.Where(h => h.IsActive && h.IsApproved)
				.OrderByDescending(h => h.BookingCount)
				.Take(take)
				.Select(h => new TopHomestayData
				{
					HomestayId = h.Id,
					HomestayTitle = h.HomestayTitle,
					City = h.City,
					HostName = h.Owner.FullName,
					BookingCount = h.BookingCount,
					AverageRating = h.RatingAverage,
					TotalRevenue = h.Bookings
						.Where(b => b.BookingStatus == BookingStatus.Completed)
						.Sum(b => b.TotalAmount),
					ReviewCount = h.TotalReviews
				})
				.ToListAsync();
		}

		public async Task<List<MonthlyBookingData>> GetMonthlyBookingDataAsync(int months)
		{
			var result = new List<MonthlyBookingData>();

			for (int i = months - 1; i >= 0; i--)
			{
				var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
				monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var monthBookings = await _context.Bookings
					.Where(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd)
					.ToListAsync();

				result.Add(new MonthlyBookingData
				{
					Month = monthStart,
					TotalBookings = monthBookings.Count,
					Completed = monthBookings.Count(b => b.BookingStatus == BookingStatus.Completed),
					Cancelled = monthBookings.Count(b => b.BookingStatus == BookingStatus.Cancelled)
				});
			}

			return result;
		}

		#endregion

		#region Revenue Statistics

		public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
		{
			var query = _context.Bookings
				.Where(b => b.BookingStatus == BookingStatus.Completed);

			if (startDate.HasValue)
			{
				query = query.Where(b => b.CreatedAt >= startDate.Value);
			}

			if (endDate.HasValue)
			{
				query = query.Where(b => b.CreatedAt <= endDate.Value);
			}

			return await query.SumAsync(b => b.TotalAmount);
		}

		public async Task<RevenueBreakdown> GetRevenueBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null)
		{
			var query = _context.Bookings
				.Where(b => b.BookingStatus == BookingStatus.Completed);

			if (startDate.HasValue)
			{
				query = query.Where(b => b.CreatedAt >= startDate.Value);
			}

			if (endDate.HasValue)
			{
				query = query.Where(b => b.CreatedAt <= endDate.Value);
			}

			var bookings = await query.ToListAsync();

			return new RevenueBreakdown
			{
				BaseAmount = bookings.Sum(b => b.BaseAmount),
				ServiceFee = bookings.Sum(b => b.ServiceFee),
				CleaningFee = bookings.Sum(b => b.CleaningFee),
				TaxAmount = bookings.Sum(b => b.TaxAmount)
			};
		}

		public async Task<List<MonthlyRevenueData>> GetMonthlyRevenueDataAsync(int months)
		{
			var result = new List<MonthlyRevenueData>();

			for (int i = months - 1; i >= 0; i--)
			{
				var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
				monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var monthBookings = await _context.Bookings
					.Where(b => b.BookingStatus == BookingStatus.Completed &&
								b.CreatedAt >= monthStart &&
								b.CreatedAt < monthEnd)
					.ToListAsync();

				result.Add(new MonthlyRevenueData
				{
					Month = monthStart,
					Revenue = monthBookings.Sum(b => b.TotalAmount),
					BookingCount = monthBookings.Count
				});
			}

			return result;
		}

		public async Task<List<PaymentMethodData>> GetPaymentMethodStatsAsync(DateTime startDate)
		{
			return await _context.Payments
				.Where(p => p.PaymentStatus == PaymentStatus.Completed &&
							p.CreatedAt >= startDate)
				.GroupBy(p => p.PaymentMethod)
				.Select(g => new PaymentMethodData
				{
					PaymentMethod = g.Key,
					TotalAmount = g.Sum(p => p.PaymentAmount),
					Count = g.Count()
				})
				.ToListAsync();
		}

		public async Task<RefundData> GetRefundDataAsync()
		{
			var refunds = await _context.Payments
				.Where(p => p.RefundAmount.HasValue && p.RefundedAt.HasValue)
				.ToListAsync();

			return new RefundData
			{
				TotalRefundAmount = refunds.Sum(p => p.RefundAmount ?? 0),
				RefundCount = refunds.Count
			};
		}

		#endregion

		#region Review Statistics

		public async Task<ReviewOverview> GetReviewOverviewAsync()
		{
			var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

			var reviews = await _context.Reviews
				.Where(r => r.IsVisible)
				.ToListAsync();

			return new ReviewOverview
			{
				AverageRating = reviews.Any() ? reviews.Average(r => r.OverallRating) : 0,
				TotalReviews = reviews.Count,
				NewReviewsThisMonth = reviews.Count(r => r.CreatedAt >= currentMonthStart),
				ComplaintCount = reviews.Count(r => r.OverallRating <= 2)
			};
		}

		public async Task<Dictionary<int, int>> GetRatingDistributionAsync()
		{
			return await _context.Reviews
				.Where(r => r.IsVisible)
				.GroupBy(r => r.OverallRating)
				.Select(g => new { Rating = g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.Rating, x => x.Count);
		}

		public async Task<List<TopHomestayData>> GetTopRatedHomestaysAsync(int take)
		{
			return await _context.Homestays
				.Include(h => h.Owner)
				.Where(h => h.TotalReviews > 0)
				.OrderByDescending(h => h.RatingAverage)
				.ThenByDescending(h => h.TotalReviews)
				.Take(take)
				.Select(h => new TopHomestayData
				{
					HomestayId = h.Id,
					HomestayTitle = h.HomestayTitle,
					City = h.City,
					HostName = h.Owner.FullName,
					AverageRating = h.RatingAverage,
					ReviewCount = h.TotalReviews,
					BookingCount = h.BookingCount,
					TotalRevenue = 0
				})
				.ToListAsync();
		}

		public async Task<List<TopHomestayData>> GetLowRatedHomestaysAsync(int take)
		{
			return await _context.Homestays
				.Include(h => h.Owner)
				.Where(h => h.TotalReviews >= 3 && h.RatingAverage < 3)
				.OrderBy(h => h.RatingAverage)
				.Take(take)
				.Select(h => new TopHomestayData
				{
					HomestayId = h.Id,
					HomestayTitle = h.HomestayTitle,
					City = h.City,
					HostName = h.Owner.FullName,
					AverageRating = h.RatingAverage,
					ReviewCount = h.TotalReviews,
					BookingCount = 0,
					TotalRevenue = 0
				})
				.ToListAsync();
		}

		public async Task<List<RecentReviewData>> GetRecentReviewsAsync(int take)
		{
			return await _context.Reviews
				.Include(r => r.Reviewer)
				.Include(r => r.Homestay)
				.Where(r => r.IsVisible)
				.OrderByDescending(r => r.CreatedAt)
				.Take(take)
				.Select(r => new RecentReviewData
				{
					ReviewId = r.Id,
					GuestName = r.Reviewer.FullName,
					HomestayTitle = r.Homestay.HomestayTitle,
					Rating = r.OverallRating,
					Comment = r.ReviewComment,
					CreatedAt = r.CreatedAt
				})
				.ToListAsync();
		}

		#endregion
	}
}