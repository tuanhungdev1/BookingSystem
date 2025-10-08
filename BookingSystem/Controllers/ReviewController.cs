using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.ReviewDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class ReviewController : ControllerBase
	{
		private readonly IReviewService _reviewService;

		public ReviewController(IReviewService reviewService)
		{
			_reviewService = reviewService;
		}

		/// <summary>
		/// Tạo review mới (Guest sau khi hoàn thành booking)
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Guest,Admin")]
		public async Task<ActionResult<ApiResponse<ReviewDto>>> Create([FromBody] CreateReviewDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<ReviewDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var review = await _reviewService.CreateReviewAsync(userId, request);

			return CreatedAtAction(
				nameof(GetById),
				new { id = review?.Id },
				new ApiResponse<ReviewDto>
				{
					Success = true,
					Message = "Review created successfully",
					Data = review
				}
			);
		}

		/// <summary>
		/// Lấy thông tin review theo ID
		/// </summary>
		[HttpGet("{id:int}")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<ReviewDto>>> GetById(int id)
		{
			var review = await _reviewService.GetByIdAsync(id);
			if (review == null)
			{
				return NotFound(new ApiResponse<ReviewDto>
				{
					Success = false,
					Message = "Review not found"
				});
			}

			return Ok(new ApiResponse<ReviewDto>
			{
				Success = true,
				Message = "Review retrieved successfully",
				Data = review
			});
		}

		/// <summary>
		/// Lấy tất cả reviews (Admin only)
		/// </summary>
		[HttpGet("all")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetAllReviews(
			[FromQuery] ReviewFilter filter)
		{
			var reviews = await _reviewService.GetAllReviewsAsync(filter);

			return Ok(new ApiResponse<PagedResult<ReviewDto>>
			{
				Success = true,
				Message = "Reviews retrieved successfully",
				Data = reviews
			});
		}

		/// <summary>
		/// Lấy reviews của homestay
		/// </summary>
		[HttpGet("homestay/{homestayId:int}")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetHomestayReviews(
			int homestayId,
			[FromQuery] ReviewFilter filter)
		{
			var reviews = await _reviewService.GetHomestayReviewsAsync(homestayId, filter);

			return Ok(new ApiResponse<PagedResult<ReviewDto>>
			{
				Success = true,
				Message = "Homestay reviews retrieved successfully",
				Data = reviews
			});
		}

		/// <summary>
		/// Lấy reviews của user (reviews đã viết và nhận được)
		/// </summary>
		[HttpGet("user/{userId:int}")]
		public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetUserReviews(
			int userId,
			[FromQuery] ReviewFilter filter)
		{
			var reviews = await _reviewService.GetUserReviewsAsync(userId, filter);

			return Ok(new ApiResponse<PagedResult<ReviewDto>>
			{
				Success = true,
				Message = "User reviews retrieved successfully",
				Data = reviews
			});
		}

		/// <summary>
		/// Lấy reviews của user hiện tại
		/// </summary>
		[HttpGet("my-reviews")]
		public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetMyReviews(
			[FromQuery] ReviewFilter filter)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<PagedResult<ReviewDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var reviews = await _reviewService.GetUserReviewsAsync(userId, filter);

			return Ok(new ApiResponse<PagedResult<ReviewDto>>
			{
				Success = true,
				Message = "Your reviews retrieved successfully",
				Data = reviews
			});
		}

		/// <summary>
		/// Cập nhật review (Reviewer hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}")]
		public async Task<ActionResult<ApiResponse<ReviewDto>>> Update(
			int id,
			[FromBody] UpdateReviewDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<ReviewDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var review = await _reviewService.UpdateReviewAsync(id, userId, request);
			if (review == null)
			{
				return NotFound(new ApiResponse<ReviewDto>
				{
					Success = false,
					Message = "Review not found"
				});
			}

			return Ok(new ApiResponse<ReviewDto>
			{
				Success = true,
				Message = "Review updated successfully",
				Data = review
			});
		}

		/// <summary>
		/// Xóa review (Reviewer hoặc Admin)
		/// </summary>
		[HttpDelete("{id:int}")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var success = await _reviewService.DeleteReviewAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to delete review"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Review deleted successfully"
			});
		}

		/// <summary>
		/// Thêm host response (Host hoặc Admin)
		/// </summary>
		[HttpPost("{id:int}/host-response")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> AddHostResponse(
			int id,
			[FromBody] HostResponseDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var success = await _reviewService.AddHostResponseAsync(id, userId, request);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to add host response"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host response added successfully"
			});
		}

		/// <summary>
		/// Cập nhật host response (Host hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/host-response")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateHostResponse(
			int id,
			[FromBody] HostResponseDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var success = await _reviewService.UpdateHostResponseAsync(id, userId, request);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to update host response"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host response updated successfully"
			});
		}

		/// <summary>
		/// Xóa host response (Host hoặc Admin)
		/// </summary>
		[HttpDelete("{id:int}/host-response")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> DeleteHostResponse(int id)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var success = await _reviewService.DeleteHostResponseAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to delete host response"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host response deleted successfully"
			});
		}

		/// <summary>
		/// Đánh dấu review là helpful
		/// </summary>
		[HttpPost("{id:int}/helpful")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<object>>> MarkAsHelpful(int id)
		{
			var success = await _reviewService.IncrementHelpfulCountAsync(id);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to mark review as helpful"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Review marked as helpful successfully"
			});
		}

		/// <summary>
		/// Toggle visibility của review (Admin only)
		/// </summary>
		[HttpPut("{id:int}/toggle-visibility")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ToggleVisibility(int id)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var success = await _reviewService.ToggleVisibilityAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to toggle review visibility"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Review visibility toggled successfully"
			});
		}

		/// <summary>
		/// Lấy thống kê reviews của homestay
		/// </summary>
		[HttpGet("homestay/{homestayId:int}/statistics")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<HomestayReviewStatistics>>> GetHomestayStatistics(int homestayId)
		{
			var statistics = await _reviewService.GetHomestayStatisticsAsync(homestayId);

			return Ok(new ApiResponse<HomestayReviewStatistics>
			{
				Success = true,
				Message = "Homestay review statistics retrieved successfully",
				Data = statistics
			});
		}

		/// <summary>
		/// Lấy thống kê reviews của user
		/// </summary>
		[HttpGet("user/{userId:int}/statistics")]
		public async Task<ActionResult<ApiResponse<UserReviewStatistics>>> GetUserStatistics(int userId)
		{
			var statistics = await _reviewService.GetUserStatisticsAsync(userId);

			return Ok(new ApiResponse<UserReviewStatistics>
			{
				Success = true,
				Message = "User review statistics retrieved successfully",
				Data = statistics
			});
		}

		/// <summary>
		/// Lấy danh sách reviews chưa có host response (Host)
		/// </summary>
		[HttpGet("pending-responses")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetPendingHostResponses()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<IEnumerable<ReviewDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var reviews = await _reviewService.GetPendingHostResponsesAsync(userId);

			return Ok(new ApiResponse<IEnumerable<ReviewDto>>
			{
				Success = true,
				Message = "Pending host responses retrieved successfully",
				Data = reviews
			});
		}
	}
}