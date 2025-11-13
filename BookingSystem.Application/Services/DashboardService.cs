// Application/Services/DashboardService.cs
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.DashboardDTO;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Repositories;

namespace BookingSystem.Application.Services
{
	public class DashboardService : IDashboardService
	{
		private readonly IDashboardRepository _dashboardRepository;
		private readonly IUserRepository _userRepository;
		private readonly IHostProfileRepository _hostProfileRepository;
		private readonly IBookingRepository _bookingRepository;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IReviewRepository _reviewRepository;
		private readonly IHostDashboardRepository _hostDashboardRepository;

		public DashboardService(
			IDashboardRepository dashboardRepository,
			IUserRepository userRepository,
			IHostProfileRepository hostProfileRepository,
			IBookingRepository bookingRepository,
			IPaymentRepository paymentRepository,
			IHostDashboardRepository hostDashboardRepository,
			IReviewRepository reviewRepository)
		{
			_dashboardRepository = dashboardRepository;
			_userRepository = userRepository;
			_hostProfileRepository = hostProfileRepository;
			_bookingRepository = bookingRepository;
			_paymentRepository = paymentRepository;
			_reviewRepository = reviewRepository;
			_hostDashboardRepository = hostDashboardRepository;
		}

		public async Task<DashboardOverviewDto> GetOverviewAsync(int months = 12)
		{
			var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
			var previousMonthStart = currentMonthStart.AddMonths(-1);

			// Get totals
			var totalUsers = await _dashboardRepository.GetTotalUsersCountAsync();
			var totalHosts = await _dashboardRepository.GetTotalHostsCountAsync();
			var totalHomestays = await _dashboardRepository.GetTotalHomestaysCountAsync();
			var activeHomestays = await _dashboardRepository.GetActiveHomestaysCountAsync();

			// Get current month counts
			var currentMonthUsers = await _dashboardRepository.GetNewUsersCountAsync(currentMonthStart, DateTime.UtcNow);
			var currentMonthHosts = await _dashboardRepository.GetNewHostsCountAsync(currentMonthStart, DateTime.UtcNow);
			var currentMonthHomestays = await _dashboardRepository.GetNewHomestaysCountAsync(currentMonthStart, DateTime.UtcNow);

			// Get previous month counts
			var previousMonthUsers = await _dashboardRepository.GetNewUsersCountAsync(previousMonthStart, currentMonthStart);
			var previousMonthHosts = await _dashboardRepository.GetNewHostsCountAsync(previousMonthStart, currentMonthStart);
			var previousMonthHomestays = await _dashboardRepository.GetNewHomestaysCountAsync(previousMonthStart, currentMonthStart);

			// Calculate growth
			var userGrowth = CalculateGrowthPercentage(currentMonthUsers, previousMonthUsers);
			var hostGrowth = CalculateGrowthPercentage(currentMonthHosts, previousMonthHosts);
			var homestayGrowth = CalculateGrowthPercentage(currentMonthHomestays, previousMonthHomestays);

			// Get monthly data
			var userMonthlyData = await GetMonthlyUserDataAsync(months);
			var hostMonthlyData = await _dashboardRepository.GetMonthlyHostDataAsync(months);
			var homestayMonthlyData = await _dashboardRepository.GetMonthlyHomestayDataAsync(months);

			return new DashboardOverviewDto
			{
				TotalUsers = totalUsers,
				TotalHosts = totalHosts,
				TotalHomestays = totalHomestays,
				ActiveHomestays = activeHomestays,
				MonthlyGrowthRate = (userGrowth + hostGrowth + homestayGrowth) / 3,
				UserGrowth = new GrowthTrendDto
				{
					CurrentMonth = currentMonthUsers,
					PreviousMonth = previousMonthUsers,
					GrowthPercentage = userGrowth,
					MonthlyData = userMonthlyData
				},
				HostGrowth = new GrowthTrendDto
				{
					CurrentMonth = currentMonthHosts,
					PreviousMonth = previousMonthHosts,
					GrowthPercentage = hostGrowth,
					MonthlyData = MapToMonthlyDataPoint(hostMonthlyData)
				},
				HomestayGrowth = new GrowthTrendDto
				{
					CurrentMonth = currentMonthHomestays,
					PreviousMonth = previousMonthHomestays,
					GrowthPercentage = homestayGrowth,
					MonthlyData = MapToMonthlyDataPoint(homestayMonthlyData)
				}
			};
		}

		public async Task<DashboardUserStatisticsDto> GetUserStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
		{
			startDate ??= DateTime.UtcNow.AddMonths(-1);
			endDate ??= DateTime.UtcNow;

			var dailyActiveUsers = await _dashboardRepository.GetActiveUsersCountAsync(30);
			var monthlyActiveUsers = await _dashboardRepository.GetActiveUsersCountAsync(30);
			var activeHosts = await _dashboardRepository.GetActiveHostsCountAsync(30);

			var totalUsers = await _dashboardRepository.GetTotalUsersCountAsync();
			var totalHosts = await _dashboardRepository.GetTotalHostsCountAsync();
			var conversionRate = totalUsers > 0 ? (decimal)totalHosts / totalUsers * 100 : 0;

			var usersByRegion = await _dashboardRepository.GetUsersByRegionAsync();
			var totalRegionUsers = usersByRegion.Values.Sum();

			var regionStats = usersByRegion.Select(kvp => new UserByRegionDto
			{
				Region = kvp.Key,
				UserCount = kvp.Value,
				HostCount = 0, // You can enhance this later
				Percentage = totalRegionUsers > 0 ? (decimal)kvp.Value / totalRegionUsers * 100 : 0
			}).ToList();

			var dailyActivity = await _dashboardRepository.GetDailyUserActivityAsync(30);
			var monthlyActivity = await GetMonthlyUserActivityAsync(12);

			return new DashboardUserStatisticsDto
			{
				DailyActiveUsers = dailyActiveUsers,
				MonthlyActiveUsers = monthlyActiveUsers,
				ActiveHosts = activeHosts,
				UserToHostConversionRate = conversionRate,
				UsersByRegion = regionStats,
				DailyActivity = dailyActivity.Select(d => new DailyActivityDto
				{
					Date = d.Date,
					ActiveUsers = d.ActiveUsers,
					NewUsers = d.NewUsers
				}).ToList(),
				MonthlyActivity = monthlyActivity
			};
		}

		public async Task<DashboardBookingStatisticsDto> GetBookingStatisticsAsync(int months = 12)
		{
			var startDate = DateTime.UtcNow.AddMonths(-months);
			var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

			var statusCount = await _dashboardRepository.GetBookingStatusCountAsync(startDate);

			// Calculate occupancy rate
			var totalHomestays = await _dashboardRepository.GetActiveHomestaysCountAsync();
			var totalBookedDays = await _dashboardRepository.GetTotalBookedDaysAsync(
				DateTime.UtcNow.AddMonths(-1),
				DateTime.UtcNow);

			var totalAvailableDays = totalHomestays * 30;
			var occupancyRate = totalAvailableDays > 0 ? (decimal)totalBookedDays / totalAvailableDays * 100 : 0;

			var newHomestays = await _dashboardRepository.GetNewHomestaysCountAsync(currentMonthStart, DateTime.UtcNow);

			var topHomestays = await _dashboardRepository.GetTopHomestaysByBookingAsync(10);
			var monthlyBookings = await _dashboardRepository.GetMonthlyBookingDataAsync(months);

			return new DashboardBookingStatisticsDto
			{
				TotalBookings = statusCount.Total,
				CompletedBookings = statusCount.Completed,
				PendingBookings = statusCount.Pending,
				CancelledBookings = statusCount.Cancelled,
				OccupancyRate = Math.Round(occupancyRate, 2),
				NewHomestaysThisMonth = newHomestays,
				StatusBreakdown = new BookingStatusBreakdownDto
				{
					Pending = statusCount.Pending,
					Confirmed = statusCount.Confirmed,
					CheckedIn = statusCount.CheckedIn,
					Completed = statusCount.Completed,
					Cancelled = statusCount.Cancelled,
					Rejected = statusCount.Rejected
				},
				TopHomestays = topHomestays.Select(h => new TopHomestayDto
				{
					HomestayId = h.HomestayId,
					HomestayTitle = h.HomestayTitle,
					City = h.City,
					BookingCount = h.BookingCount,
					AverageRating = h.AverageRating,
					TotalRevenue = h.TotalRevenue
				}).ToList(),
				MonthlyBookings = monthlyBookings.Select(m => new MonthlyBookingDto
				{
					Month = m.Month.ToString("MMM yyyy"),
					TotalBookings = m.TotalBookings,
					Completed = m.Completed,
					Cancelled = m.Cancelled
				}).ToList()
			};
		}

		public async Task<RevenueStatisticsDto> GetRevenueStatisticsAsync(int months = 12)
		{
			var startDate = DateTime.UtcNow.AddMonths(-months);
			var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
			var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);

			var totalRevenue = await _dashboardRepository.GetTotalRevenueAsync();
			var monthlyRevenue = await _dashboardRepository.GetTotalRevenueAsync(currentMonthStart, DateTime.UtcNow);
			var yearlyRevenue = await _dashboardRepository.GetTotalRevenueAsync(yearStart, DateTime.UtcNow);

			var monthlyRevenueData = await _dashboardRepository.GetMonthlyRevenueDataAsync(months);
			var avgRevenuePerBooking = monthlyRevenueData.Sum(m => m.BookingCount) > 0
				? monthlyRevenueData.Sum(m => m.Revenue) / monthlyRevenueData.Sum(m => m.BookingCount)
				: 0;

			var refundData = await _dashboardRepository.GetRefundDataAsync();
			var refundRate = totalRevenue > 0 ? refundData.TotalRefundAmount / totalRevenue * 100 : 0;

			var revenueBreakdown = await _dashboardRepository.GetRevenueBreakdownAsync(startDate, DateTime.UtcNow);
			var paymentMethodStats = await _dashboardRepository.GetPaymentMethodStatsAsync(startDate);

			var totalPayments = paymentMethodStats.Sum(p => p.TotalAmount);

			return new RevenueStatisticsDto
			{
				TotalRevenue = totalRevenue,
				MonthlyRevenue = monthlyRevenue,
				YearlyRevenue = yearlyRevenue,
				AverageRevenuePerBooking = avgRevenuePerBooking,
				RefundAmount = refundData.TotalRefundAmount,
				RefundRate = refundRate,
				MonthlyRevenueData = monthlyRevenueData.Select(m => new MonthlyRevenueDto
				{
					Month = m.Month.ToString("MMM yyyy"),
					Revenue = m.Revenue,
					BookingCount = m.BookingCount,
					AveragePerBooking = m.BookingCount > 0 ? m.Revenue / m.BookingCount : 0
				}).ToList(),
				RevenueBreakdown = new RevenueBreakdownDto
				{
					BaseAmount = revenueBreakdown.BaseAmount,
					ServiceFee = revenueBreakdown.ServiceFee,
					CleaningFee = revenueBreakdown.CleaningFee,
					TaxAmount = revenueBreakdown.TaxAmount
				},
				PaymentMethodStats = paymentMethodStats.Select(p => new PaymentMethodStatsDto
				{
					PaymentMethod = p.PaymentMethod.ToString(),
					TotalAmount = p.TotalAmount,
					Count = p.Count,
					Percentage = totalPayments > 0 ? p.TotalAmount / totalPayments * 100 : 0
				}).ToList()
			};
		}

		public async Task<ReviewStatisticsDto> GetReviewStatisticsAsync(int months = 6)
		{
			var reviewOverview = await _dashboardRepository.GetReviewOverviewAsync();
			var ratingDistribution = await _dashboardRepository.GetRatingDistributionAsync();
			var topRated = await _dashboardRepository.GetTopRatedHomestaysAsync(10);
			var lowRated = await _dashboardRepository.GetLowRatedHomestaysAsync(10);
			var recentReviews = await _dashboardRepository.GetRecentReviewsAsync(10);

			return new ReviewStatisticsDto
			{
				AverageRating = Math.Round(reviewOverview.AverageRating, 2),
				TotalReviews = reviewOverview.TotalReviews,
				NewReviewsThisMonth = reviewOverview.NewReviewsThisMonth,
				ComplaintCount = reviewOverview.ComplaintCount,
				RatingDistribution = new RatingDistributionDto
				{
					FiveStar = ratingDistribution.GetValueOrDefault(5, 0),
					FourStar = ratingDistribution.GetValueOrDefault(4, 0),
					ThreeStar = ratingDistribution.GetValueOrDefault(3, 0),
					TwoStar = ratingDistribution.GetValueOrDefault(2, 0),
					OneStar = ratingDistribution.GetValueOrDefault(1, 0)
				},
				TopRatedHomestays = topRated.Select(h => new TopRatedHomestayDto
				{
					HomestayId = h.HomestayId,
					HomestayTitle = h.HomestayTitle,
					HostName = h.HostName,
					AverageRating = h.AverageRating,
					ReviewCount = h.ReviewCount
				}).ToList(),
				LowRatedHomestays = lowRated.Select(h => new LowRatedHomestayDto
				{
					HomestayId = h.HomestayId,
					HomestayTitle = h.HomestayTitle,
					HostName = h.HostName,
					AverageRating = h.AverageRating,
					ReviewCount = h.ReviewCount,
					MostCommonComplaint = null
				}).ToList(),
				RecentReviews = recentReviews.Select(r => new RecentReviewDto
				{
					ReviewId = r.ReviewId,
					GuestName = r.GuestName,
					HomestayTitle = r.HomestayTitle,
					Rating = r.Rating,
					Comment = r.Comment,
					CreatedAt = r.CreatedAt
				}).ToList()
			};
		}

		#region Helper Methods

		private decimal CalculateGrowthPercentage(int current, int previous)
		{
			if (previous == 0) return current > 0 ? 100 : 0;
			return Math.Round((decimal)(current - previous) / previous * 100, 2);
		}

		private async Task<List<MonthlyDataPoint>> GetMonthlyUserDataAsync(int months)
		{
			var result = new List<MonthlyDataPoint>();

			for (int i = months - 1; i >= 0; i--)
			{
				var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
				monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var count = await _dashboardRepository.GetNewUsersCountAsync(monthStart, monthEnd);

				result.Add(new MonthlyDataPoint
				{
					Month = monthStart.ToString("MMM yyyy"),
					Value = count
				});
			}

			return result;
		}

		private async Task<List<DailyActivityDto>> GetMonthlyUserActivityAsync(int months)
		{
			var result = new List<DailyActivityDto>();

			for (int i = months - 1; i >= 0; i--)
			{
				var monthStart = DateTime.UtcNow.AddMonths(-i).Date;
				monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var activeUsers = await _dashboardRepository.GetActiveUsersCountAsync(30);
				var newUsers = await _dashboardRepository.GetNewUsersCountAsync(monthStart, monthEnd);

				result.Add(new DailyActivityDto
				{
					Date = monthStart,
					ActiveUsers = activeUsers,
					NewUsers = newUsers
				});
			}

			return result;
		}

		private List<MonthlyDataPoint> MapToMonthlyDataPoint<T>(List<T> data) where T : class
		{
			var result = new List<MonthlyDataPoint>();

			if (typeof(T) == typeof(MonthlyHostData))
			{
				var hostData = data as List<MonthlyHostData>;
				result = hostData?.Select(d => new MonthlyDataPoint
				{
					Month = d.Month.ToString("MMM yyyy"),
					Value = d.Count
				}).ToList() ?? new List<MonthlyDataPoint>();
			}
			else if (typeof(T) == typeof(MonthlyHomestayData))
			{
				var homestayData = data as List<MonthlyHomestayData>;
				result = homestayData?.Select(d => new MonthlyDataPoint
				{
					Month = d.Month.ToString("MMM yyyy"),
					Value = d.Count
				}).ToList() ?? new List<MonthlyDataPoint>();
			}

			return result;
		}

		#endregion

		#region Host Dashboard Methods

		public async Task<HostDashboardOverviewDto> GetHostOverviewAsync(int hostId)
		{
			var overview = await _hostDashboardRepository.GetHostOverviewAsync(hostId);
			var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
			var previousMonth = currentMonth.AddMonths(-1);

			// Get current and previous month stats for growth calculation
			var currentRevenue = await _hostDashboardRepository.GetHostRevenueStatsAsync(
				hostId, currentMonth, DateTime.UtcNow);
			var previousRevenue = await _hostDashboardRepository.GetHostRevenueStatsAsync(
				hostId, previousMonth, currentMonth);

			var revenueGrowth = CalculateGrowthPercentage(
				currentRevenue.TotalRevenue,
				previousRevenue.TotalRevenue);

			return new HostDashboardOverviewDto
			{
				TotalHomestays = overview.TotalHomestays,
				ActiveHomestays = overview.ActiveHomestays,
				TotalBookings = overview.TotalBookings,
				TotalRevenue = overview.TotalRevenue,
				AverageRating = Math.Round(overview.AverageRating, 2),
				TotalReviews = overview.TotalReviews,
				RevenueGrowth = revenueGrowth,
				MonthlyRevenue = currentRevenue.TotalRevenue
			};
		}

		public async Task<HostRevenueStatisticsDto> GetHostRevenueStatisticsAsync(
			int hostId,
			int months = 12)
		{
			var startDate = DateTime.UtcNow.AddMonths(-months);
			var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
			var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);

			var totalRevenue = await _hostDashboardRepository.GetHostRevenueStatsAsync(
				hostId, DateTime.MinValue, DateTime.UtcNow);
			var monthlyRevenue = await _hostDashboardRepository.GetHostRevenueStatsAsync(
				hostId, currentMonthStart, DateTime.UtcNow);
			var yearlyRevenue = await _hostDashboardRepository.GetHostRevenueStatsAsync(
			hostId, yearStart, DateTime.UtcNow);

			var monthlyData = await _hostDashboardRepository.GetHostMonthlyRevenueAsync(hostId, months);

			return new HostRevenueStatisticsDto
			{
				TotalRevenue = totalRevenue.TotalRevenue,
				MonthlyRevenue = monthlyRevenue.TotalRevenue,
				YearlyRevenue = yearlyRevenue.TotalRevenue,
				AverageBookingValue = totalRevenue.AverageBookingValue,
				RevenueBreakdown = new HostRevenueBreakdownDto
				{
					BaseRevenue = totalRevenue.BaseRevenue,
					CleaningFees = totalRevenue.CleaningFees,
					ServiceFees = totalRevenue.ServiceFees,
					TaxAmount = totalRevenue.TaxAmount
				},
				MonthlyRevenueData = monthlyData.Select(m => new MonthlyHostRevenueDto
				{
					Month = m.Month.ToString("MMM yyyy"),
					Revenue = m.Revenue,
					BookingCount = m.BookingCount,
					GuestCount = m.GuestCount
				}).ToList()
			};
		}

		public async Task<HostBookingStatisticsDto> GetHostBookingStatisticsAsync(
			int hostId,
			int months = 12)
		{
			var startDate = DateTime.UtcNow.AddMonths(-months);
			var bookingStats = await _hostDashboardRepository.GetHostBookingStatsAsync(
			hostId, startDate, DateTime.UtcNow);

			var bookingTrends = await _hostDashboardRepository.GetHostBookingTrendsAsync(hostId, months);
			var occupancyData = await _hostDashboardRepository.GetHostOccupancyRateAsync(
				hostId, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

			return new HostBookingStatisticsDto
			{
				TotalBookings = bookingStats.TotalBookings,
				PendingBookings = bookingStats.Pending,
				ConfirmedBookings = bookingStats.Confirmed,
				CompletedBookings = bookingStats.Completed,
				CancelledBookings = bookingStats.Cancelled,
				CancellationRate = Math.Round(bookingStats.CancellationRate, 2),
				OccupancyRate = Math.Round(occupancyData.OccupancyRate, 2),
				StatusBreakdown = new HostBookingStatusDto
				{
					Pending = bookingStats.Pending,
					Confirmed = bookingStats.Confirmed,
					CheckedIn = bookingStats.CheckedIn,
					Completed = bookingStats.Completed,
					Cancelled = bookingStats.Cancelled,
					Rejected = bookingStats.Rejected
				},
				MonthlyTrends = bookingTrends.Select(t => new MonthlyBookingTrendDto
				{
					Month = t.Month.ToString("MMM yyyy"),
					TotalBookings = t.TotalBookings,
					Completed = t.Completed,
					Cancelled = t.Cancelled,
					Pending = t.Pending
				}).ToList()
			};
		}

		public async Task<HostReviewStatisticsDto> GetHostReviewStatisticsAsync(int hostId)
		{
			var reviews = await _hostDashboardRepository.GetHostReviewsAsync(hostId, 20);
			var overview = await _hostDashboardRepository.GetHostOverviewAsync(hostId);

			var ratingBreakdown = reviews
				.GroupBy(r => r.OverallRating)
				.ToDictionary(g => g.Key, g => g.Count());

			return new HostReviewStatisticsDto
			{
				AverageRating = Math.Round(overview.AverageRating, 2),
				TotalReviews = overview.TotalReviews,
				AverageCleanlinessRating = reviews.Any()
					? Math.Round(reviews.Average(r => (double)r.CleanlinessRating), 2)
					: 0,
				AverageAccuracyRating = reviews.Any()
					? Math.Round(reviews.Average(r => (double)r.AccuracyRating), 2)
					: 0,
				AverageCommunicationRating = reviews.Any()
					? Math.Round(reviews.Average(r => (double)r.CommunicationRating), 2)
					: 0,
				AverageLocationRating = reviews.Any()
					? Math.Round(reviews.Average(r => (double)r.LocationRating), 2)
					: 0,
				AverageValueRating = reviews.Any()
					? Math.Round(reviews.Average(r => (double)r.ValueRating), 2)
					: 0,
				RatingDistribution = new HostRatingDistributionDto
				{
					FiveStar = ratingBreakdown.GetValueOrDefault(5, 0),
					FourStar = ratingBreakdown.GetValueOrDefault(4, 0),
					ThreeStar = ratingBreakdown.GetValueOrDefault(3, 0),
					TwoStar = ratingBreakdown.GetValueOrDefault(2, 0),
					OneStar = ratingBreakdown.GetValueOrDefault(1, 0)
				},
				RecentReviews = reviews.Select(r => new HostRecentReviewDto
				{
					ReviewId = r.ReviewId,
					GuestName = r.GuestName,
					HomestayTitle = r.HomestayTitle,
					Rating = r.OverallRating,
					Comment = r.Comment,
					CreatedAt = r.CreatedAt
				}).ToList()
			};
		}

		public async Task<HostPerformanceDto> GetHostPerformanceAsync(
		int hostId,
			int months = 12)
		{
			var homestayPerformance = await _hostDashboardRepository.GetHostHomestaysPerformanceAsync(hostId);
			var topGuests = await _hostDashboardRepository.GetHostTopGuestsAsync(hostId, 10);
			var calendarData = await _hostDashboardRepository.GetHostCalendarDataAsync(
				hostId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(3));

			return new HostPerformanceDto
			{
				HomestayPerformance = homestayPerformance.Select(h => new HomestayPerformanceDto
				{
					HomestayId = h.HomestayId,
					HomestayTitle = h.HomestayTitle,
					BookingCount = h.BookingCount,
					Revenue = h.Revenue,
					AverageRating = h.AverageRating,
					ReviewCount = h.ReviewCount,
					OccupancyRate = h.OccupancyRate,
					ViewCount = h.ViewCount
				}).ToList(),
				TopGuests = topGuests.Select(g => new TopGuestDto
				{
					GuestId = g.GuestId,
					GuestName = g.GuestName,
					Email = g.Email,
					TotalBookings = g.TotalBookings,
					TotalSpent = g.TotalSpent,
					LastBookingDate = g.LastBookingDate
				}).ToList(),
				UpcomingBookings = calendarData.Bookings.Select(b => new UpcomingBookingDto
				{
					BookingId = b.BookingId,
					HomestayTitle = b.HomestayTitle,
					GuestName = b.GuestName,
					CheckInDate = b.CheckInDate,
					CheckOutDate = b.CheckOutDate,
					Status = b.Status,
					TotalAmount = b.TotalAmount
				}).ToList()
			};
		}

		private decimal CalculateGrowthPercentage(decimal current, decimal previous)
		{
			if (previous == 0)
				return current > 0 ? 100 : 0;

			return Math.Round(((current - previous) / previous) * 100, 2);
		}

		#endregion
	}
}