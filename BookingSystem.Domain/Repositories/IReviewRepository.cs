using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base.Shared;
using BookingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Repositories
{
	public interface IReviewRepository : IRepository<Review>
	{
		Task<Review?> GetByIdWithDetailsAsync(int id);
		Task<PagedResult<Review>> GetAllReviewsAsync(ReviewFilter filter);
		Task<PagedResult<Review>> GetHomestayReviewsAsync(int homestayId, ReviewFilter filter);
		Task<PagedResult<Review>> GetUserReviewsAsync(int userId, ReviewFilter filter);
		Task<PagedResult<Review>> GetReviewsByReviewerAsync(int reviewerId, ReviewFilter filter);
		Task<PagedResult<Review>> GetReviewsByRevieweeAsync(int revieweeId, ReviewFilter filter);
		Task<PagedResult<Review>> GetReviewsByHostIdAsync(int hostId, ReviewFilter filter);
		Task<Review?> GetByBookingIdAsync(int bookingId);
		Task<bool> HasUserReviewedBookingAsync(int bookingId, int userId);
		Task<HomestayReviewStatistics> GetHomestayReviewStatisticsAsync(int homestayId);
		Task<UserReviewStatistics> GetUserReviewStatisticsAsync(int userId);
		Task<IEnumerable<Review>> GetPendingHostResponsesAsync(int hostId);
		Task<bool> ToggleHelpfulAsync(int userId, int reviewId);
		Task<bool> HasUserMarkedHelpfulAsync(int userId, int reviewId);
	}
}
