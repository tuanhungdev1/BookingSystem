using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base.Shared;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
	public class ReviewRepository : Repository<Review>, IReviewRepository
	{
		public ReviewRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<bool> HasUserMarkedHelpfulAsync(int userId, int reviewId)
		{
			return await _context.Set<ReviewHelpful>()
				.AnyAsync(rh => rh.UserId == userId && rh.ReviewId == reviewId);
		}

		public async Task<bool> ToggleHelpfulAsync(int userId, int reviewId)
		{
			var review = await _dbSet
				.Include(r => r.ReviewHelpfuls)
				.FirstOrDefaultAsync(r => r.Id == reviewId && r.IsVisible);

			if (review == null)
				throw new NotFoundException($"Review with ID {reviewId} not found or not visible.");

			var helpful = await _context.Set<ReviewHelpful>()
				.FirstOrDefaultAsync(rh => rh.UserId == userId && rh.ReviewId == reviewId);

			if (helpful != null)
			{
				// HỦY (Unlike)
				_context.Set<ReviewHelpful>().Remove(helpful);
				review.HelpfulCount--;
			}
			else
			{
				// THÊM (Like)
				_context.Set<ReviewHelpful>().Add(new ReviewHelpful
				{
					UserId = userId,
					ReviewId = reviewId
				});
				review.HelpfulCount++;
			}

			review.UpdatedAt = DateTime.UtcNow;
			_dbSet.Update(review);

			// Chỉ SaveChanges, KHÔNG bắt transaction
			await _context.SaveChangesAsync();

			return helpful == null; // true = đã thêm, false = đã hủy
		}

		public async Task<Review?> GetByIdWithDetailsAsync(int id)
		{
			return await _dbSet
				.Include(r => r.Reviewer)
				.Include(r => r.Reviewee)
				.Include(r => r.Booking)
				.Include(r => r.Homestay)
					.ThenInclude(h => h.Owner)
				.FirstOrDefaultAsync(r => r.Id == id);
		}

		public async Task<PagedResult<Review>> GetReviewsByHostIdAsync(int hostId, ReviewFilter filter)
		{
			var query = _dbSet
				.Include(r => r.Reviewer)
				.Include(r => r.Reviewee)
				.Include(r => r.Homestay)
					.ThenInclude(h => h.Owner)
				.Include(r => r.Booking)
				.Where(r => r.Homestay.OwnerId == hostId) // Chỉ lấy review của homestay do host sở hữu
				.AsQueryable();

			// Áp dụng filter (giống như GetAllReviewsAsync)
			query = ApplyFilters(query, filter);
			query = ApplySorting(query, filter);

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Review>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize
			};
		}

		public async Task<PagedResult<Review>> GetAllReviewsAsync(ReviewFilter filter)
		{
			var query = _dbSet
				.Include(r => r.Reviewer)
				.Include(r => r.Reviewee)
				.Include(r => r.Homestay)
				.Include(r => r.Booking)
				.AsQueryable();

			query = ApplyFilters(query, filter);
			query = ApplySorting(query, filter);

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Review>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize
			};
		}

		public async Task<PagedResult<Review>> GetHomestayReviewsAsync(int homestayId, ReviewFilter filter)
		{
			filter.HomestayId = homestayId;
			return await GetAllReviewsAsync(filter);
		}

		public async Task<PagedResult<Review>> GetUserReviewsAsync(int userId, ReviewFilter filter)
		{
			var query = _dbSet
				.Include(r => r.Reviewer)
				.Include(r => r.Reviewee)
				.Include(r => r.Homestay)
				.Include(r => r.Booking)
				.Where(r => r.ReviewerId == userId || r.RevieweeId == userId)
				.AsQueryable();

			query = ApplyFilters(query, filter);
			query = ApplySorting(query, filter);

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Review>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize
			};
		}

		public async Task<PagedResult<Review>> GetReviewsByReviewerAsync(int reviewerId, ReviewFilter filter)
		{
			filter.ReviewerId = reviewerId;
			return await GetAllReviewsAsync(filter);
		}

		public async Task<PagedResult<Review>> GetReviewsByRevieweeAsync(int revieweeId, ReviewFilter filter)
		{
			filter.RevieweeId = revieweeId;
			return await GetAllReviewsAsync(filter);
		}

		public async Task<Review?> GetByBookingIdAsync(int bookingId)
		{
			return await _dbSet
				.Include(r => r.Reviewer)
				.Include(r => r.Reviewee)
				.Include(r => r.Homestay)
				.Include(r => r.Booking)
				.FirstOrDefaultAsync(r => r.BookingId == bookingId);
		}

		public async Task<bool> HasUserReviewedBookingAsync(int bookingId, int userId)
		{
			return await _dbSet.AnyAsync(r => r.BookingId == bookingId && r.ReviewerId == userId);
		}

		public async Task<HomestayReviewStatistics> GetHomestayReviewStatisticsAsync(int homestayId)
		{
			var reviews = await _dbSet
				.Where(r => r.HomestayId == homestayId && r.IsVisible)
				.ToListAsync();

			if (!reviews.Any())
			{
				return new HomestayReviewStatistics();
			}

			var totalReviews = reviews.Count;
			var recommendedCount = reviews.Count(r => r.IsRecommended);

			var ratingDistribution = reviews
				.GroupBy(r => r.OverallRating)
				.ToDictionary(g => g.Key, g => g.Count());

			return new HomestayReviewStatistics
			{
				TotalReviews = totalReviews,
				AverageOverallRating = Math.Round(reviews.Average(r => r.OverallRating), 2),
				AverageCleanlinessRating = Math.Round(reviews.Average(r => r.CleanlinessRating), 2),
				AverageLocationRating = Math.Round(reviews.Average(r => r.LocationRating), 2),
				AverageValueForMoneyRating = Math.Round(reviews.Average(r => r.ValueForMoneyRating), 2),
				AverageCommunicationRating = Math.Round(reviews.Average(r => r.CommunicationRating), 2),
				AverageCheckInRating = Math.Round(reviews.Average(r => r.CheckInRating), 2),
				RecommendedCount = recommendedCount,
				RecommendationPercentage = Math.Round((double)recommendedCount / totalReviews * 100, 2),
				RatingDistribution = ratingDistribution
			};
		}

		public async Task<UserReviewStatistics> GetUserReviewStatisticsAsync(int userId)
		{
			var reviewsWritten = await _dbSet
				.Where(r => r.ReviewerId == userId)
				.CountAsync();

			var reviewsReceived = await _dbSet
				.Where(r => r.RevieweeId == userId && r.IsVisible)
				.ToListAsync();

			var totalReceived = reviewsReceived.Count;
			var recommendedCount = reviewsReceived.Count(r => r.IsRecommended);
			var averageRating = totalReceived > 0
				? Math.Round(reviewsReceived.Average(r => r.OverallRating), 2)
				: 0;

			return new UserReviewStatistics
			{
				TotalReviewsWritten = reviewsWritten,
				TotalReviewsReceived = totalReceived,
				AverageRatingReceived = averageRating,
				RecommendedCount = recommendedCount,
				RecommendationPercentage = totalReceived > 0
					? Math.Round((double)recommendedCount / totalReceived * 100, 2)
					: 0
			};
		}

		public async Task<IEnumerable<Review>> GetPendingHostResponsesAsync(int hostId)
		{
			return await _dbSet
				.Include(r => r.Reviewer)
				.Include(r => r.Homestay)
				.Include(r => r.Booking)
				.Where(r => r.Homestay.OwnerId == hostId
					&& r.IsVisible
					&& string.IsNullOrEmpty(r.HostResponse))
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();
		}

		private IQueryable<Review> ApplyFilters(IQueryable<Review> query, ReviewFilter filter)
		{
			if (filter.HomestayId.HasValue)
			{
				query = query.Where(r => r.HomestayId == filter.HomestayId.Value);
			}

			if (filter.ReviewerId.HasValue)
			{
				query = query.Where(r => r.ReviewerId == filter.ReviewerId.Value);
			}

			if (filter.RevieweeId.HasValue)
			{
				query = query.Where(r => r.RevieweeId == filter.RevieweeId.Value);
			}

			if (filter.BookingId.HasValue)
			{
				query = query.Where(r => r.BookingId == filter.BookingId.Value);
			}

			if (filter.MinOverallRating.HasValue)
			{
				query = query.Where(r => r.OverallRating >= filter.MinOverallRating.Value);
			}

			if (filter.MaxOverallRating.HasValue)
			{
				query = query.Where(r => r.OverallRating <= filter.MaxOverallRating.Value);
			}

			if (filter.IsVisible.HasValue)
			{
				query = query.Where(r => r.IsVisible == filter.IsVisible.Value);
			}

			if (filter.IsRecommended.HasValue)
			{
				query = query.Where(r => r.IsRecommended == filter.IsRecommended.Value);
			}

			if (filter.HasHostResponse.HasValue)
			{
				query = filter.HasHostResponse.Value
					? query.Where(r => !string.IsNullOrEmpty(r.HostResponse))
					: query.Where(r => string.IsNullOrEmpty(r.HostResponse));
			}

			if (filter.CreatedFrom.HasValue)
			{
				query = query.Where(r => r.CreatedAt >= filter.CreatedFrom.Value);
			}

			if (filter.CreatedTo.HasValue)
			{
				query = query.Where(r => r.CreatedAt <= filter.CreatedTo.Value);
			}

			if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
			{
				query = query.Where(r =>
					r.ReviewComment!.Contains(filter.SearchTerm) ||
					r.Reviewer.FirstName.Contains(filter.SearchTerm) ||
					r.Reviewer.LastName.Contains(filter.SearchTerm));
			}

			return query;
		}

		private IQueryable<Review> ApplySorting(IQueryable<Review> query, ReviewFilter filter)
		{
			query = filter.SortBy?.ToLower() switch
			{
				"rating" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(r => r.OverallRating)
					: query.OrderBy(r => r.OverallRating),
				"helpful" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(r => r.HelpfulCount)
					: query.OrderBy(r => r.HelpfulCount),
				_ => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(r => r.CreatedAt)
					: query.OrderBy(r => r.CreatedAt)
			};

			return query;
		}
	}
}