using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.ReviewDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using BookingSystem.Domain.Base.Shared;

namespace BookingSystem.Application.Services
{
	public class ReviewService : IReviewService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IReviewRepository _reviewRepository;
		private readonly IBookingRepository _bookingRepository;
		private readonly IHomestayRepository _homestayRepository;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<ReviewService> _logger;

		public ReviewService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IReviewRepository reviewRepository,
			IBookingRepository bookingRepository,
			IHomestayRepository homestayRepository,
			UserManager<User> userManager,
			ILogger<ReviewService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_reviewRepository = reviewRepository;
			_bookingRepository = bookingRepository;
			_homestayRepository = homestayRepository;
			_userManager = userManager;
			_logger = logger;
		}

		public async Task<HelpfulToggleResult> ToggleHelpfulCountAsync(int userId, int reviewId)
		{
			_logger.LogInformation("User {UserId} toggling helpful for review {ReviewId}.", userId, reviewId);

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
				throw new NotFoundException($"User with ID {userId} not found.");

			// Kiểm tra quyền: chỉ guest đã đăng nhập
			//var roles = await _userManager.GetRolesAsync(user);
			//if (!roles.Contains("Host") && !roles.Contains("Admin"))
			//	throw new BadRequestException("Only authenticated users can mark reviews as helpful.");

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var isNowHelpful = await _reviewRepository.ToggleHelpfulAsync(userId, reviewId);

				await _unitOfWork.CommitTransactionAsync();

				var review = await _reviewRepository.GetByIdAsync(reviewId);
				return new HelpfulToggleResult
				{
					IsNowHelpful = isNowHelpful,
					HelpfulCount = review!.HelpfulCount
				};
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();
				_logger.LogError(ex, "Error toggling helpful for review {ReviewId}", reviewId);
				throw;
			}
		}

		public async Task<PagedResult<ReviewDto>> GetReviewsByHostIdAsync(int hostId, ReviewFilter filter)
		{
			_logger.LogInformation("Fetching all reviews for homestays owned by host {HostId}.", hostId);

			// Kiểm tra host tồn tại
			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			// Kiểm tra quyền
			var roles = await _userManager.GetRolesAsync(host);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				throw new BadRequestException("User does not have Host or Admin role.");
			}

			// Lấy danh sách review
			var pagedReviews = await _reviewRepository.GetReviewsByHostIdAsync(hostId, filter);
			var reviewDtos = _mapper.Map<List<ReviewDto>>(pagedReviews.Items);

			_logger.LogInformation("Retrieved {Count} reviews for host {HostId}.", reviewDtos.Count, hostId);

			return new PagedResult<ReviewDto>
			{
				Items = reviewDtos,
				TotalCount = pagedReviews.TotalCount,
				PageNumber = pagedReviews.PageNumber,
				PageSize = pagedReviews.PageSize
			};
		}

		public async Task<ReviewDto> CreateReviewAsync(int reviewerId, CreateReviewDto request)
		{
			_logger.LogInformation("Creating review for booking {BookingId} by reviewer {ReviewerId}.",
				request.BookingId, reviewerId);

			// Validate reviewer
			var reviewer = await _userManager.FindByIdAsync(reviewerId.ToString());
			if (reviewer == null)
			{
				throw new NotFoundException($"Reviewer with ID {reviewerId} not found.");
			}

			// Validate booking
			var booking = await _bookingRepository.GetByIdWithDetailsAsync(request.BookingId);
			if (booking == null)
			{
				throw new NotFoundException($"Booking with ID {request.BookingId} not found.");
			}

			// Verify reviewer is the guest
			if (booking.GuestId != reviewerId)
			{
				throw new BadRequestException("You can only review your own bookings.");
			}

			// Check booking status
			if (booking.BookingStatus != BookingStatus.Completed && booking.BookingStatus != BookingStatus.CheckedOut)
			{
				throw new BadRequestException("You can only review completed bookings.");
			}

			// Check if already reviewed
			var existingReview = await _reviewRepository.HasUserReviewedBookingAsync(request.BookingId, reviewerId);
			if (existingReview)
			{
				throw new BadRequestException("You have already reviewed this booking.");
			}

			// Validate ratings (1-5)
			ValidateRatings(request.OverallRating, request.CleanlinessRating, request.LocationRating,
				request.ValueForMoneyRating, request.CommunicationRating, request.CheckInRating);

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var review = new Review
				{
					OverallRating = request.OverallRating,
					CleanlinessRating = request.CleanlinessRating,
					LocationRating = request.LocationRating,
					ValueForMoneyRating = request.ValueForMoneyRating,
					CommunicationRating = request.CommunicationRating,
					CheckInRating = request.CheckInRating,
					ReviewComment = request.ReviewComment,
					IsRecommended = request.IsRecommended,
					IsVisible = true,
					ReviewerId = reviewerId,
					RevieweeId = booking.Homestay.OwnerId,
					BookingId = request.BookingId,
					HomestayId = booking.HomestayId,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _reviewRepository.AddAsync(review);
				await _reviewRepository.SaveChangesAsync();

				await RecalculateHomestayRatingAsync(review.HomestayId);

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Review {ReviewId} created successfully.", review.Id);

				// Reload with details
				var savedReview = await _reviewRepository.GetByIdWithDetailsAsync(review.Id);
				return _mapper.Map<ReviewDto>(savedReview);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating review.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<ReviewDto?> UpdateReviewAsync(int reviewId, int userId, UpdateReviewDto request)
		{
			_logger.LogInformation("Updating review {ReviewId} by user {UserId}.", reviewId, userId);

			var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
			if (review == null)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isReviewer = review.ReviewerId == userId;

			// Only reviewer or admin can update
			if (!isAdmin && !isReviewer)
			{
				throw new BadRequestException("You do not have permission to update this review.");
			}

			// Check if review is too old (e.g., 7 days)
			if (!isAdmin && (DateTime.UtcNow - review.CreatedAt).TotalDays > 7)
			{
				throw new BadRequestException("Reviews can only be edited within 7 days of posting.");
			}

			// Validate ratings if provided
			if (request.OverallRating.HasValue)
				ValidateRating(request.OverallRating.Value, nameof(request.OverallRating));
			if (request.CleanlinessRating.HasValue)
				ValidateRating(request.CleanlinessRating.Value, nameof(request.CleanlinessRating));
			if (request.LocationRating.HasValue)
				ValidateRating(request.LocationRating.Value, nameof(request.LocationRating));
			if (request.ValueForMoneyRating.HasValue)
				ValidateRating(request.ValueForMoneyRating.Value, nameof(request.ValueForMoneyRating));
			if (request.CommunicationRating.HasValue)
				ValidateRating(request.CommunicationRating.Value, nameof(request.CommunicationRating));
			if (request.CheckInRating.HasValue)
				ValidateRating(request.CheckInRating.Value, nameof(request.CheckInRating));

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Update fields
				if (request.OverallRating.HasValue)
					review.OverallRating = request.OverallRating.Value;
				if (request.CleanlinessRating.HasValue)
					review.CleanlinessRating = request.CleanlinessRating.Value;
				if (request.LocationRating.HasValue)
					review.LocationRating = request.LocationRating.Value;
				if (request.ValueForMoneyRating.HasValue)
					review.ValueForMoneyRating = request.ValueForMoneyRating.Value;
				if (request.CommunicationRating.HasValue)
					review.CommunicationRating = request.CommunicationRating.Value;
				if (request.CheckInRating.HasValue)
					review.CheckInRating = request.CheckInRating.Value;
				if (request.ReviewComment != null)
					review.ReviewComment = request.ReviewComment;
				if (request.IsRecommended.HasValue)
					review.IsRecommended = request.IsRecommended.Value;

				review.UpdatedAt = DateTime.UtcNow;
				_reviewRepository.Update(review);
				await _reviewRepository.SaveChangesAsync();
				await RecalculateHomestayRatingAsync(review.HomestayId);
				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Review {ReviewId} updated successfully.", reviewId);

				return _mapper.Map<ReviewDto>(review);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating review {ReviewId}.", reviewId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
		{
			_logger.LogInformation("Deleting review {ReviewId} by user {UserId}.", reviewId, userId);

			var review = await _reviewRepository.GetByIdAsync(reviewId);
			if (review == null)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains("Admin");
			var isReviewer = review.ReviewerId == userId;

			// Only reviewer or admin can delete
			if (!isAdmin && !isReviewer)
			{
				throw new BadRequestException("You do not have permission to delete this review.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();
				var homestayId = review.HomestayId;

				_reviewRepository.Remove(review);
				await _reviewRepository.SaveChangesAsync();
				await RecalculateHomestayRatingAsync(homestayId);
				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Review {ReviewId} deleted successfully.", reviewId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting review {ReviewId}.", reviewId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<ReviewDto?> GetByIdAsync(int reviewId)
		{
			var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
			if (review == null)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			// Only show visible reviews unless admin
			if (!review.IsVisible)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			return _mapper.Map<ReviewDto>(review);
		}

		public async Task<PagedResult<ReviewDto>> GetAllReviewsAsync(ReviewFilter filter)
		{
			var pagedReviews = await _reviewRepository.GetAllReviewsAsync(filter);
			var reviewDtos = _mapper.Map<List<ReviewDto>>(pagedReviews.Items);

			return new PagedResult<ReviewDto>
			{
				Items = reviewDtos,
				TotalCount = pagedReviews.TotalCount,
				PageSize = pagedReviews.PageSize,
				PageNumber = pagedReviews.PageNumber
			};
		}

		public async Task<PagedResult<ReviewDto>> GetHomestayReviewsAsync(int homestayId, ReviewFilter filter)
		{
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			// Only show visible reviews for public viewing
			filter.IsVisible = true;

			var pagedReviews = await _reviewRepository.GetHomestayReviewsAsync(homestayId, filter);
			var reviewDtos = _mapper.Map<List<ReviewDto>>(pagedReviews.Items);

			return new PagedResult<ReviewDto>
			{
				Items = reviewDtos,
				TotalCount = pagedReviews.TotalCount,
				PageSize = pagedReviews.PageSize,
				PageNumber = pagedReviews.PageNumber
			};
		}

		public async Task<PagedResult<ReviewDto>> GetUserReviewsAsync(int userId, ReviewFilter filter)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var pagedReviews = await _reviewRepository.GetUserReviewsAsync(userId, filter);
			var reviewDtos = _mapper.Map<List<ReviewDto>>(pagedReviews.Items);

			return new PagedResult<ReviewDto>
			{
				Items = reviewDtos,
				TotalCount = pagedReviews.TotalCount,
				PageSize = pagedReviews.PageSize,
				PageNumber = pagedReviews.PageNumber
			};
		}

		public async Task<bool> AddHostResponseAsync(int reviewId, int hostId, HostResponseDto request)
		{
			_logger.LogInformation("Adding host response to review {ReviewId} by host {HostId}.", reviewId, hostId);

			var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
			if (review == null)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			var isAdmin = roles.Contains("Admin");
			var isHostOwner = review.Homestay.OwnerId == hostId;

			// Only homestay owner or admin can respond
			if (!isAdmin && !isHostOwner)
			{
				throw new BadRequestException("You do not have permission to respond to this review.");
			}

			// Check if host has already responded
			if (!string.IsNullOrEmpty(review.HostResponse))
			{
				throw new BadRequestException("Host has already responded to this review. Use update instead.");
			}

			if (string.IsNullOrWhiteSpace(request.HostResponse))
			{
				throw new BadRequestException("Host response cannot be empty.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				review.HostResponse = request.HostResponse;
				review.HostRespondedAt = DateTime.UtcNow;
				review.UpdatedAt = DateTime.UtcNow;

				_reviewRepository.Update(review);
				await _reviewRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Host response added to review {ReviewId} successfully.", reviewId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while adding host response to review {ReviewId}.", reviewId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> UpdateHostResponseAsync(int reviewId, int hostId, HostResponseDto request)
		{
			_logger.LogInformation("Updating host response for review {ReviewId} by host {HostId}.", reviewId, hostId);

			var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
			if (review == null)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			var isAdmin = roles.Contains("Admin");
			var isHostOwner = review.Homestay.OwnerId == hostId;

			// Only homestay owner or admin can update response
			if (!isAdmin && !isHostOwner)
			{
				throw new BadRequestException("You do not have permission to update this response.");
			}

			// Check if host has responded
			if (string.IsNullOrEmpty(review.HostResponse))
			{
				throw new BadRequestException("No host response exists. Use add instead.");
			}

			if (string.IsNullOrWhiteSpace(request.HostResponse))
			{
				throw new BadRequestException("Host response cannot be empty.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				review.HostResponse = request.HostResponse;
				review.UpdatedAt = DateTime.UtcNow;

				_reviewRepository.Update(review);
				await _reviewRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Host response updated for review {ReviewId} successfully.", reviewId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating host response for review {ReviewId}.", reviewId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> DeleteHostResponseAsync(int reviewId, int hostId)
		{
			_logger.LogInformation("Deleting host response for review {ReviewId} by host {HostId}.", reviewId, hostId);

			var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
			if (review == null)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			var isAdmin = roles.Contains("Admin");
			var isHostOwner = review.Homestay.OwnerId == hostId;

			// Only homestay owner or admin can delete response
			if (!isAdmin && !isHostOwner)
			{
				throw new BadRequestException("You do not have permission to delete this response.");
			}

			// Check if host has responded
			if (string.IsNullOrEmpty(review.HostResponse))
			{
				throw new BadRequestException("No host response exists to delete.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				review.HostResponse = null;
				review.HostRespondedAt = null;
				review.UpdatedAt = DateTime.UtcNow;

				_reviewRepository.Update(review);
				await _reviewRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Host response deleted for review {ReviewId} successfully.", reviewId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting host response for review {ReviewId}.", reviewId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> ToggleVisibilityAsync(int reviewId, int adminId)
		{
			_logger.LogInformation("Toggling visibility for review {ReviewId} by admin {AdminId}.", reviewId, adminId);

			var review = await _reviewRepository.GetByIdAsync(reviewId);
			if (review == null)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			var admin = await _userManager.FindByIdAsync(adminId.ToString());
			if (admin == null)
			{
				throw new NotFoundException($"Admin with ID {adminId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(admin);
			if (!roles.Contains("Admin"))
			{
				throw new BadRequestException("Only admins can toggle review visibility.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				review.IsVisible = !review.IsVisible;
				review.UpdatedAt = DateTime.UtcNow;

				_reviewRepository.Update(review);
				await _reviewRepository.SaveChangesAsync();
				await RecalculateHomestayRatingAsync(review.HomestayId);
				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Review {ReviewId} visibility toggled to {IsVisible}.",
					reviewId, review.IsVisible);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while toggling visibility for review {ReviewId}.", reviewId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> IncrementHelpfulCountAsync(int reviewId)
		{
			_logger.LogInformation("Incrementing helpful count for review {ReviewId}.", reviewId);

			var review = await _reviewRepository.GetByIdAsync(reviewId);
			if (review == null)
			{
				throw new NotFoundException($"Review with ID {reviewId} not found.");
			}

			if (!review.IsVisible)
			{
				throw new BadRequestException("Cannot mark a hidden review as helpful.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				review.HelpfulCount++;
				review.UpdatedAt = DateTime.UtcNow;

				_reviewRepository.Update(review);
				await _reviewRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Helpful count incremented for review {ReviewId}.", reviewId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while incrementing helpful count for review {ReviewId}.", reviewId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<HomestayReviewStatistics> GetHomestayStatisticsAsync(int homestayId)
		{
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			return await _reviewRepository.GetHomestayReviewStatisticsAsync(homestayId);
		}

		public async Task<UserReviewStatistics> GetUserStatisticsAsync(int userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			return await _reviewRepository.GetUserReviewStatisticsAsync(userId);
		}

		public async Task<IEnumerable<ReviewDto>> GetPendingHostResponsesAsync(int hostId)
		{
			var host = await _userManager.FindByIdAsync(hostId.ToString());
			if (host == null)
			{
				throw new NotFoundException($"Host with ID {hostId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(host);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				throw new BadRequestException("User does not have Host or Admin role.");
			}

			var pendingReviews = await _reviewRepository.GetPendingHostResponsesAsync(hostId);
			return _mapper.Map<IEnumerable<ReviewDto>>(pendingReviews);
		}

		private void ValidateRatings(int overall, int cleanliness, int location,
			int valueForMoney, int communication, int checkIn)
		{
			ValidateRating(overall, nameof(overall));
			ValidateRating(cleanliness, nameof(cleanliness));
			ValidateRating(location, nameof(location));
			ValidateRating(valueForMoney, nameof(valueForMoney));
			ValidateRating(communication, nameof(communication));
			ValidateRating(checkIn, nameof(checkIn));
		}

		private async Task RecalculateHomestayRatingAsync(int homestayId)
		{
			_logger.LogInformation("Recalculating rating for homestay {HomestayId}.", homestayId);

			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay {HomestayId} not found for rating recalculation.", homestayId);
				return;
			}

			// Lấy tất cả review visible của homestay này
			var reviews = await _reviewRepository.GetHomestayReviewsAsync(homestayId, new ReviewFilter
			{
				IsVisible = true,
				PageNumber = 1,
				PageSize = int.MaxValue // Lấy tất cả
			});

			// Tính toán
			var visibleReviews = reviews.Items.Where(r => r.IsVisible).ToList();
			homestay.TotalReviews = visibleReviews.Count;

			if (visibleReviews.Any())
			{
				homestay.RatingAverage = Math.Round(
					visibleReviews.Average(r => r.OverallRating),
					1 // Làm tròn 1 chữ số thập phân
				);
			}
			else
			{
				homestay.RatingAverage = 0.0;
			}

			_homestayRepository.Update(homestay);
			await _homestayRepository.SaveChangesAsync();

			_logger.LogInformation(
				"Homestay {HomestayId} rating updated: {Rating} ({Count} reviews).",
				homestayId, homestay.RatingAverage, homestay.TotalReviews
			);
		}

		private void ValidateRating(int rating, string ratingName)
		{
			if (rating < 1 || rating > 5)
			{
				throw new BadRequestException($"{ratingName} must be between 1 and 5.");
			}
		}

	}
}
