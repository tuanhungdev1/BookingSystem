using BookingSystem.Application.DTOs.ReviewDTO;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Shared;

namespace BookingSystem.Application.Contracts
{
	public interface IReviewService
	{
		Task<ReviewDto> CreateReviewAsync(int reviewerId, CreateReviewDto request);
		Task<ReviewDto?> UpdateReviewAsync(int reviewId, int userId, UpdateReviewDto request);
		Task<bool> DeleteReviewAsync(int reviewId, int userId);
		Task<ReviewDto?> GetByIdAsync(int reviewId);
		Task<PagedResult<ReviewDto>> GetAllReviewsAsync(ReviewFilter filter);
		Task<PagedResult<ReviewDto>> GetHomestayReviewsAsync(int homestayId, ReviewFilter filter);
		Task<PagedResult<ReviewDto>> GetUserReviewsAsync(int userId, ReviewFilter filter);
		Task<PagedResult<ReviewDto>> GetReviewsByHostIdAsync(int hostId, ReviewFilter filter);
		Task<bool> AddHostResponseAsync(int reviewId, int hostId, HostResponseDto request);
		Task<bool> UpdateHostResponseAsync(int reviewId, int hostId, HostResponseDto request);
		Task<bool> DeleteHostResponseAsync(int reviewId, int hostId);
		Task<bool> ToggleVisibilityAsync(int reviewId, int adminId);
		Task<bool> IncrementHelpfulCountAsync(int reviewId);
		Task<HomestayReviewStatistics> GetHomestayStatisticsAsync(int homestayId);
		Task<UserReviewStatistics> GetUserStatisticsAsync(int userId);
		Task<IEnumerable<ReviewDto>> GetPendingHostResponsesAsync(int hostId);
		Task<HelpfulToggleResult> ToggleHelpfulCountAsync(int userId, int reviewId);
	}
}
