using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.DashboardDTO;
using BookingSystem.Application.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/host/dashboard")]
	//[Authorize(Roles = "Host")]
	public class HostDashboardController : ControllerBase
	{
		private readonly IDashboardService _dashboardService;
		private readonly ILogger<HostDashboardController> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public HostDashboardController(
			IDashboardService dashboardService,
			ILogger<HostDashboardController> logger,
			IHttpContextAccessor httpContextAccessor)
		{
			_dashboardService = dashboardService;
			_logger = logger;
			_httpContextAccessor = httpContextAccessor;
		}

		private int GetCurrentHostId()
		{
			var userIdClaim = _httpContextAccessor.HttpContext?.User
				.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
			{
				throw new UnauthorizedAccessException("Invalid user ID");
			}

			return userId;
		}

		[HttpGet("overview")]
		[ProducesResponseType(typeof(ApiResponse<HostDashboardOverviewDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<HostDashboardOverviewDto>>> GetOverview()
		{
			try
			{
				var hostId = GetCurrentHostId();
				var overview = await _dashboardService.GetHostOverviewAsync(hostId);

				return Ok(new ApiResponse<HostDashboardOverviewDto>
				{
					Success = true,
					Message = "Host dashboard overview retrieved successfully",
					Data = overview
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting host dashboard overview");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching dashboard overview"
				});
			}
		}

		[HttpGet("revenue")]
		[ProducesResponseType(typeof(ApiResponse<HostRevenueStatisticsDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<HostRevenueStatisticsDto>>> GetRevenueStatistics(
			[FromQuery] int months = 12)
		{
			try
			{
				var hostId = GetCurrentHostId();
				var stats = await _dashboardService.GetHostRevenueStatisticsAsync(hostId, months);

				return Ok(new ApiResponse<HostRevenueStatisticsDto>
				{
					Success = true,
					Message = "Revenue statistics retrieved successfully",
					Data = stats
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting host revenue statistics");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching revenue statistics"
				});
			}
		}

		[HttpGet("bookings")]
		[ProducesResponseType(typeof(ApiResponse<HostBookingStatisticsDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<HostBookingStatisticsDto>>> GetBookingStatistics(
			[FromQuery] int months = 12)
		{
			try
			{
				var hostId = GetCurrentHostId();
				var stats = await _dashboardService.GetHostBookingStatisticsAsync(hostId, months);

				return Ok(new ApiResponse<HostBookingStatisticsDto>
				{
					Success = true,
					Message = "Booking statistics retrieved successfully",
					Data = stats
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting host booking statistics");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching booking statistics"
				});
			}
		}

		[HttpGet("reviews")]
		[ProducesResponseType(typeof(ApiResponse<HostReviewStatisticsDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<HostReviewStatisticsDto>>> GetReviewStatistics()
		{
			try
			{
				var hostId = GetCurrentHostId();
				var stats = await _dashboardService.GetHostReviewStatisticsAsync(hostId);

				return Ok(new ApiResponse<HostReviewStatisticsDto>
				{
					Success = true,
					Message = "Review statistics retrieved successfully",
					Data = stats
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting host review statistics");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching review statistics"
				});
			}
		}

		[HttpGet("performance")]
		[ProducesResponseType(typeof(ApiResponse<HostPerformanceDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<HostPerformanceDto>>> GetPerformance(
			[FromQuery] int months = 12)
		{
			try
			{
				var hostId = GetCurrentHostId();
				var performance = await _dashboardService.GetHostPerformanceAsync(hostId, months);

				return Ok(new ApiResponse<HostPerformanceDto>
				{
					Success = true,
					Message = "Performance data retrieved successfully",
					Data = performance
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting host performance");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching performance data"
				});
			}
		}

		[HttpGet("complete")]
		[ProducesResponseType(typeof(ApiResponse<CompleteHostDashboardDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<CompleteHostDashboardDto>>> GetCompleteDashboard(
			[FromQuery] int months = 12)
		{
			try
			{
				var hostId = GetCurrentHostId();

				var overview = await _dashboardService.GetHostOverviewAsync(hostId);
				var revenue = await _dashboardService.GetHostRevenueStatisticsAsync(hostId, months);
				var bookings = await _dashboardService.GetHostBookingStatisticsAsync(hostId, months);
				var reviews = await _dashboardService.GetHostReviewStatisticsAsync(hostId);
				var performance = await _dashboardService.GetHostPerformanceAsync(hostId, months);

				var completeDashboard = new CompleteHostDashboardDto
				{
					Overview = overview,
					Revenue = revenue,
					Bookings = bookings,
					Reviews = reviews,
					Performance = performance,
					GeneratedAt = DateTime.UtcNow
				};

				return Ok(new ApiResponse<CompleteHostDashboardDto>
				{
					Success = true,
					Message = "Complete dashboard data retrieved successfully",
					Data = completeDashboard
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting complete host dashboard");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching complete dashboard"
				});
			}
		}
	}

	public class CompleteHostDashboardDto
	{
		public HostDashboardOverviewDto Overview { get; set; } = new();
		public HostRevenueStatisticsDto Revenue { get; set; } = new();
		public HostBookingStatisticsDto Bookings { get; set; } = new();
		public HostReviewStatisticsDto Reviews { get; set; } = new();
		public HostPerformanceDto Performance { get; set; } = new();
		public DateTime GeneratedAt { get; set; }
	}
}
