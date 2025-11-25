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
		private readonly IGenericExportService _exportService;
		public ReviewController(IReviewService reviewService, IGenericExportService exportService)
		{
			_reviewService = reviewService;
			_exportService = exportService;
		}

		private int GetCurrentUserId()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdClaim))
				throw new UnauthorizedAccessException("User ID not found in token.");

			return int.Parse(userIdClaim);
		}

		// ============================== EXPORT REVIEWS (Admin only) ==============================

		[HttpGet("export/excel")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ExportReviewsToExcel([FromQuery] ReviewFilter filter)
		{
			try
			{
				var result = await _reviewService.GetAllReviewsAsync(filter);

				var exportData = result.Items.Select(r => new ReviewExportDto
				{
					Id = r.Id,
					BookingId = r.BookingId,
					HomestayId = r.HomestayId,
					HomestayTitle = r.HomestayTitle,
					ReviewerId = r.ReviewerId,
					ReviewerName = r.ReviewerName,
					RevieweeId = r.RevieweeId,
					RevieweeName = r.RevieweeName,
					OverallRating = r.OverallRating,
					CleanlinessRating = r.CleanlinessRating,
					LocationRating = r.LocationRating,
					ValueForMoneyRating = r.ValueForMoneyRating,
					CommunicationRating = r.CommunicationRating,
					CheckInRating = r.CheckInRating,
					ReviewComment = r.ReviewComment ?? "N/A",
					HostResponse = r.HostResponse ?? "N/A",
					HostRespondedAt = r.HostRespondedAt,
					HelpfulCount = r.HelpfulCount,
					IsRecommended = r.IsRecommended,
					IsVisible = r.IsVisible,
					CreatedAt = r.CreatedAt,
					UpdatedAt = r.UpdatedAt
				});

				var fileBytes = await _exportService.ExportToExcelAsync(
					exportData,
					sheetName: "Danh sách đánh giá",
					fileName: "reviews.xlsx"
				);

				return File(fileBytes,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					$"reviews_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Export Excel thất bại: {ex.Message}"
				});
			}
		}

		[HttpGet("export/pdf")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ExportReviewsToPdf([FromQuery] ReviewFilter filter)
		{
			try
			{
				var result = await _reviewService.GetAllReviewsAsync(filter);

				var exportData = result.Items.Select(r => new ReviewExportDto
				{
					Id = r.Id,
					BookingId = r.BookingId,
					HomestayId = r.HomestayId,
					HomestayTitle = r.HomestayTitle,
					ReviewerId = r.ReviewerId,
					ReviewerName = r.ReviewerName,
					RevieweeId = r.RevieweeId,
					RevieweeName = r.RevieweeName,
					OverallRating = r.OverallRating,
					CleanlinessRating = r.CleanlinessRating,
					LocationRating = r.LocationRating,
					ValueForMoneyRating = r.ValueForMoneyRating,
					CommunicationRating = r.CommunicationRating,
					CheckInRating = r.CheckInRating,
					ReviewComment = r.ReviewComment ?? "N/A",
					HostResponse = r.HostResponse ?? "N/A",
					HostRespondedAt = r.HostRespondedAt,
					HelpfulCount = r.HelpfulCount,
					IsRecommended = r.IsRecommended,
					IsVisible = r.IsVisible,
					CreatedAt = r.CreatedAt,
					UpdatedAt = r.UpdatedAt
				});

				var fileBytes = await _exportService.ExportToPdfAsync(
					exportData,
					title: "BÁO CÁO ĐÁNH GIÁ - BOOKING SYSTEM",
					fileName: "reviews.pdf"
				);

				return File(fileBytes, "application/pdf", $"reviews_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Export PDF thất bại: {ex.Message}"
				});
			}
		}

		[HttpGet("export/csv")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ExportReviewsToCSV([FromQuery] ReviewFilter filter)
		{
			try
			{
				var result = await _reviewService.GetAllReviewsAsync(filter);

				var exportData = result.Items.Select(r => new ReviewExportDto
				{
					Id = r.Id,
					BookingId = r.BookingId,
					HomestayId = r.HomestayId,
					HomestayTitle = r.HomestayTitle,
					ReviewerId = r.ReviewerId,
					ReviewerName = r.ReviewerName,
					RevieweeId = r.RevieweeId,
					RevieweeName = r.RevieweeName,
					OverallRating = r.OverallRating,
					CleanlinessRating = r.CleanlinessRating,
					LocationRating = r.LocationRating,
					ValueForMoneyRating = r.ValueForMoneyRating,
					CommunicationRating = r.CommunicationRating,
					CheckInRating = r.CheckInRating,
					ReviewComment = r.ReviewComment ?? "N/A",
					HostResponse = r.HostResponse ?? "N/A",
					HostRespondedAt = r.HostRespondedAt,
					HelpfulCount = r.HelpfulCount,
					IsRecommended = r.IsRecommended,
					IsVisible = r.IsVisible,
					CreatedAt = r.CreatedAt,
					UpdatedAt = r.UpdatedAt
				});

				var fileBytes = await _exportService.ExportToCSVAsync(
					exportData,
					fileName: "reviews.csv"
				);

				return File(fileBytes, "text/csv", $"reviews_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Export CSV thất bại: {ex.Message}"
				});
			}
		}

		[HttpPost("{reviewId:int}/helpful")]
		[Authorize]
		public async Task<ActionResult<ApiResponse<HelpfulToggleResult>>> ToggleHelpful(int reviewId)
		{
			var userId = GetCurrentUserId();
			var result = await _reviewService.ToggleHelpfulCountAsync(userId, reviewId);

			return Ok(new ApiResponse<HelpfulToggleResult>
			{
				Success = true,
				Message = result.IsNowHelpful ? "Marked as helpful" : "Helpful mark removed",
				Data = result
			});
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

		[HttpGet("host")]
		[Authorize(Roles = "Host, Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetReviewsByHost(
		[FromQuery] ReviewFilter filter)
		{
			var hostId = GetCurrentUserId();
			var reviews = await _reviewService.GetReviewsByHostIdAsync(hostId, filter);

			return Ok(new ApiResponse<PagedResult<ReviewDto>>
			{
				Success = true,
				Message = "Host reviews retrieved successfully",
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
		[HttpPost("{id:int}/helpful-old")]
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