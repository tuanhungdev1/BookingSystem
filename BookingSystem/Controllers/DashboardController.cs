using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.DashboardDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Application.Services;
using BookingSystem.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/admin/dashboard")]
	[Authorize(Policy = "Admin")]
	public class DashboardController : ControllerBase
	{
		private readonly IDashboardService _dashboardService;
		private readonly ILogger<DashboardController> _logger;
		private readonly IDashboardExportService _dashboardExportService;

		public DashboardController(
			IDashboardService dashboardService,
			IDashboardExportService dashboardExportService,
			ILogger<DashboardController> logger)
		{
			_dashboardService = dashboardService;
			_logger = logger;
			_dashboardExportService = dashboardExportService;
		}

		[HttpGet("overview")]
		[ProducesResponseType(typeof(ApiResponse<DashboardOverviewDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> GetOverview([FromQuery] int months = 12)
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

				var overview = await _dashboardService.GetOverviewAsync(months);
				return Ok(new ApiResponse<DashboardOverviewDto>
				{
					Success = true,
					Message = "Dashboard overview retrieved successfully",
					Data = overview
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting dashboard overview");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching dashboard overview"
				});
			}
		}

		[HttpGet("users")]
		[ProducesResponseType(typeof(ApiResponse<DashboardUserStatisticsDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<DashboardUserStatisticsDto>>> GetUserStatistics(
			[FromQuery] DateTime? startDate = null,
			[FromQuery] DateTime? endDate = null)
		{
			try
			{
				if (startDate.HasValue && endDate.HasValue && startDate > endDate)
				{
					return BadRequest(new ApiResponse
					{
						Success = false,
						Message = "Start date must be before end date"
					});
				}

				var stats = await _dashboardService.GetUserStatisticsAsync(startDate, endDate);
				return Ok(new ApiResponse<DashboardUserStatisticsDto>
				{
					Success = true,
					Message = "User statistics retrieved successfully",
					Data = stats
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user statistics");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching user statistics"
				});
			}
		}

		[HttpGet("bookings")]
		[ProducesResponseType(typeof(ApiResponse<DashboardBookingStatisticsDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<DashboardBookingStatisticsDto>>> GetBookingStatistics([FromQuery] int months = 12)
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

				var stats = await _dashboardService.GetBookingStatisticsAsync(months);
				return Ok(new ApiResponse<DashboardBookingStatisticsDto>
				{
					Success = true,
					Message = "Booking statistics retrieved successfully",
					Data = stats
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting booking statistics");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching booking statistics"
				});
			}
		}

		[HttpGet("revenue")]
		[ProducesResponseType(typeof(ApiResponse<RevenueStatisticsDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<RevenueStatisticsDto>>> GetRevenueStatistics([FromQuery] int months = 12)
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

				var stats = await _dashboardService.GetRevenueStatisticsAsync(months);
				return Ok(new ApiResponse<RevenueStatisticsDto>
				{
					Success = true,
					Message = "Revenue statistics retrieved successfully",
					Data = stats
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting revenue statistics");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching revenue statistics"
				});
			}
		}

		[HttpGet("reviews")]
		[ProducesResponseType(typeof(ApiResponse<ReviewStatisticsDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<ReviewStatisticsDto>>> GetReviewStatistics([FromQuery] int months = 6)
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

				var stats = await _dashboardService.GetReviewStatisticsAsync(months);
				return Ok(new ApiResponse<ReviewStatisticsDto>
				{
					Success = true,
					Message = "Review statistics retrieved successfully",
					Data = stats
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting review statistics");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching review statistics"
				});
			}
		}

		[HttpGet("complete")]
		[ProducesResponseType(typeof(ApiResponse<CompleteDashboardDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<CompleteDashboardDto>>> GetCompleteDashboard([FromQuery] int months = 12)
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

				var overview = await _dashboardService.GetOverviewAsync(months);
				var userStats = await _dashboardService.GetUserStatisticsAsync();
				var bookingStats = await _dashboardService.GetBookingStatisticsAsync(months);
				var revenueStats = await _dashboardService.GetRevenueStatisticsAsync(months);
				var reviewStats = await _dashboardService.GetReviewStatisticsAsync(6);

				var completeDashboard = new CompleteDashboardDto
				{
					Overview = overview,
					UserStatistics = userStats,
					BookingStatistics = bookingStats,
					RevenueStatistics = revenueStats,
					ReviewStatistics = reviewStats,
					GeneratedAt = DateTime.UtcNow
				};

				return Ok(new ApiResponse<CompleteDashboardDto>
				{
					Success = true,
					Message = "Complete dashboard data retrieved successfully",
					Data = completeDashboard
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting complete dashboard");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while fetching complete dashboard"
				});
			}
		}

		//[HttpGet("export")]
		//[ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
		//[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		//[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
		//public async Task<IActionResult> ExportDashboard(
		//	[FromQuery] string format = "excel",
		//	[FromQuery] int months = 12)
		//{
		//	try
		//	{
		//		if (format.ToLower() != "excel" && format.ToLower() != "csv")
		//		{
		//			return BadRequest(new ApiResponse
		//			{
		//				Success = false,
		//				Message = "Format must be 'excel' or 'csv'"
		//			});
		//		}

		//		// TODO: Implement export logic (EPPlus, CSV, etc.)
		//		return BadRequest(new ApiResponse
		//		{
		//			Success = false,
		//			Message = "Export functionality not yet implemented"
		//		});
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "Error exporting dashboard");
		//		return StatusCode(500, new ApiResponse
		//		{
		//			Success = false,
		//			Message = "An error occurred while exporting dashboard"
		//		});
		//	}
		//}

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

				// Lấy complete dashboard data
				var overview = await _dashboardService.GetOverviewAsync(months);
				var userStats = await _dashboardService.GetUserStatisticsAsync();
				var bookingStats = await _dashboardService.GetBookingStatisticsAsync(months);
				var revenueStats = await _dashboardService.GetRevenueStatisticsAsync(months);
				var reviewStats = await _dashboardService.GetReviewStatisticsAsync(6);

				var completeDashboard = new CompleteDashboardDto
				{
					Overview = overview,
					UserStatistics = userStats,
					BookingStatistics = bookingStats,
					RevenueStatistics = revenueStats,
					ReviewStatistics = reviewStats,
					GeneratedAt = DateTime.UtcNow
				};

				// Export theo format
				byte[] fileBytes;
				string contentType;
				string fileName;

				switch (format.ToLower())
				{
					case "excel":
						fileBytes = await _dashboardExportService.ExportAdminDashboardToExcelAsync(completeDashboard, months);
						contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
						fileName = $"Admin_Dashboard_{DateTime.Now:yyyy-MM-dd}.xlsx";
						break;

					case "pdf":
						fileBytes = await _dashboardExportService.ExportAdminDashboardToPdfAsync(completeDashboard, months);
						contentType = "application/pdf";
						fileName = $"Admin_Dashboard_{DateTime.Now:yyyy-MM-dd}.pdf";
						break;

					case "csv":
						fileBytes = await _dashboardExportService.ExportAdminDashboardToCSVAsync(completeDashboard, months);
						contentType = "text/csv";
						fileName = $"Admin_Dashboard_{DateTime.Now:yyyy-MM-dd}.csv";
						break;

					default:
						return BadRequest(new ApiResponse
						{
							Success = false,
							Message = "Invalid format"
						});
				}

				_logger.LogInformation("Dashboard exported as {Format} by admin", format);
				return File(fileBytes, contentType, fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting dashboard");
				return StatusCode(500, new ApiResponse
				{
					Success = false,
					Message = "An error occurred while exporting dashboard"
				});
			}
		}
	}

	// DTO for complete dashboard
	//public class CompleteDashboardDto
	//{
	//	public DashboardOverviewDto Overview { get; set; } = new();
	//	public DashboardUserStatisticsDto UserStatistics { get; set; } = new();
	//	public DashboardBookingStatisticsDto BookingStatistics { get; set; } = new();
	//	public RevenueStatisticsDto RevenueStatistics { get; set; } = new();
	//	public ReviewStatisticsDto ReviewStatistics { get; set; } = new();
	//	public DateTime GeneratedAt { get; set; }
	//}
}