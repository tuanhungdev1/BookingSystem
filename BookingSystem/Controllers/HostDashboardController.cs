using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.BookingDTO;
using BookingSystem.Application.DTOs.DashboardDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Application.Services;
using BookingSystem.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/host/dashboard")]
	[Authorize(Policy = "Host")]
	public class HostDashboardController : ControllerBase
	{
		private readonly IDashboardService _dashboardService;
		private readonly ILogger<HostDashboardController> _logger;
		private readonly IDashboardExportService _dashboardExportService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public HostDashboardController(
			IDashboardService dashboardService,
			ILogger<HostDashboardController> logger,
			IDashboardExportService dashboardExportService,
			IHttpContextAccessor httpContextAccessor)
		{
			_dashboardService = dashboardService;
			_logger = logger;
			_httpContextAccessor = httpContextAccessor;
			_dashboardExportService = dashboardExportService;
		}

		private int GetCurrentHostId()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
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

		[HttpGet("export")]
		[ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> ExportDashboard(
	[FromQuery] string format = "excel",
	[FromQuery] int months = 12)
		{
			try
			{
				if (months < 1 || months > 24)
				{
					return BadRequest(new ApiResponse
					{
						Success = false,
						Message = "Months must be between 1 and 24"
					});
				}

				if (!format.ToLower().IsValidExportFormat())
				{
					return BadRequest(new ApiResponse
					{
						Success = false,
						Message = "Format must be 'excel', 'pdf', or 'csv'"
					});
				}

				var hostId = GetCurrentHostId();

				// Lấy complete host dashboard data
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

				// Export theo format
				byte[] fileBytes;
				string contentType;
				string fileName;

				switch (format.ToLower())
				{
					case "excel":
						fileBytes = await _dashboardExportService.ExportHostDashboardToExcelAsync(completeDashboard, months);
						contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
						fileName = $"Host_Dashboard_{DateTime.Now:yyyy-MM-dd}.xlsx";
						break;

					case "pdf":
						fileBytes = await _dashboardExportService.ExportHostDashboardToPdfAsync(completeDashboard, months);
						contentType = "application/pdf";
						fileName = $"Host_Dashboard_{DateTime.Now:yyyy-MM-dd}.pdf";
						break;

					case "csv":
						fileBytes = await _dashboardExportService.ExportHostDashboardToCSVAsync(completeDashboard, months);
						contentType = "text/csv";
						fileName = $"Host_Dashboard_{DateTime.Now:yyyy-MM-dd}.csv";
						break;

					default:
						return BadRequest(new ApiResponse
						{
							Success = false,
							Message = "Invalid format"
						});
				}

				_logger.LogInformation("Host dashboard exported as {Format} by host {HostId}", format, hostId);
				return File(fileBytes, contentType, fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting host dashboard");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while exporting dashboard"
				});
			}
		}

	}

	//public class CompleteHostDashboardDto
	//{
	//	public HostDashboardOverviewDto Overview { get; set; } = new();
	//	public HostRevenueStatisticsDto Revenue { get; set; } = new();
	//	public HostBookingStatisticsDto Bookings { get; set; } = new();
	//	public HostReviewStatisticsDto Reviews { get; set; } = new();
	//	public HostPerformanceDto Performance { get; set; } = new();
	//	public DateTime GeneratedAt { get; set; }
	//}
}
