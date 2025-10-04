using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.HostProfileDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class HostProfileController : ControllerBase
	{
		private readonly IHostProfileService _hostProfileService;

		public HostProfileController(IHostProfileService hostProfileService)
		{
			_hostProfileService = hostProfileService;
		}

		/// <summary>
		/// Đăng ký làm Host (User authenticated)
		/// </summary>
		[HttpPost("register")]
		public async Task<ActionResult<ApiResponse<object>>> RegisterHost([FromForm] CreateHostProfileDto request)
		{
			var success = await _hostProfileService.RegisterHostAsync(request);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host profile registration submitted successfully. Pending admin approval."
			});
		}

		/// <summary>
		/// Lấy thông tin Host Profile theo User ID (Host hoặc Admin)
		/// </summary>
		[HttpGet("{userId:int}")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<HostProfileDto>>> GetHostProfileById(int userId)
		{
			var hostProfile = await _hostProfileService.GetHostProfileByIdAsync(userId);
			if (hostProfile == null)
			{
				return NotFound(new ApiResponse<HostProfileDto>
				{
					Success = false,
					Message = "Host profile not found"
				});
			}

			return Ok(new ApiResponse<HostProfileDto>
			{
				Success = true,
				Message = "Host profile retrieved successfully",
				Data = hostProfile
			});
		}

		/// <summary>
		/// Lấy danh sách Host Profile có phân trang và lọc (Admin only)
		/// </summary>
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<HostProfileDto>>>> GetAllHostProfiles([FromQuery] HostProfileFilter filter)
		{
			var hostProfiles = await _hostProfileService.GetAllHostProfileAsync(filter);
			return Ok(new ApiResponse<PagedResult<HostProfileDto>>
			{
				Success = true,
				Message = "Host profiles retrieved successfully",
				Data = hostProfiles
			});
		}

		/// <summary>
		/// Cập nhật thông tin Host Profile (Host only)
		/// </summary>
		[HttpPut("{id:int}")]
		[Authorize(Roles = "Host")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateHostProfile(int id, [FromBody] UpdateHostProfileDto request)
		{
			var success = await _hostProfileService.UpdateHostProfileAsync(id, request);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "Host profile not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host profile updated successfully"
			});
		}

		/// <summary>
		/// Upload ảnh CMND/CCCD mặt trước và sau (Host only)
		/// </summary>
		[HttpPost("{hostId:int}/identity-card")]
		[Authorize(Roles = "Host")]
		public async Task<ActionResult<ApiResponse<object>>> UploadIdentityCard(
			int hostId,
			[FromForm] IFormFile frontFile,
			[FromForm] IFormFile backFile)
		{
			var message = await _hostProfileService.UploadIdentityCardAsync(hostId, frontFile, backFile);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = message
			});
		}

		/// <summary>
		/// Upload ảnh giấy phép kinh doanh (Host only)
		/// </summary>
		[HttpPost("{hostId:int}/business-license")]
		[Authorize(Roles = "Host")]
		public async Task<ActionResult<ApiResponse<object>>> UploadBusinessLicense(
			int hostId,
			[FromForm] IFormFile file)
		{
			var message = await _hostProfileService.UploadBusinessLicenseAsync(hostId, file);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = message
			});
		}

		/// <summary>
		/// Upload ảnh giấy tờ mã số thuế (Host only)
		/// </summary>
		[HttpPost("{hostId:int}/tax-code-document")]
		[Authorize(Roles = "Host")]
		public async Task<ActionResult<ApiResponse<object>>> UploadTaxCodeDocument(
			int hostId,
			[FromForm] IFormFile file)
		{
			var message = await _hostProfileService.UploadTaxCodeDocumentAsync(hostId, file);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = message
			});
		}

		/// <summary>
		/// Duyệt Host Profile (Admin only)
		/// </summary>
		[HttpPost("{id:int}/approve")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ApproveHostProfile(
			int id,
			[FromBody] ApproveHostProfileRequest request)
		{
			var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out var adminId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid admin credentials"
				});
			}

			var success = await _hostProfileService.ApproveHostProfileAsync(id, adminId, request?.Note);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host profile approved successfully"
			});
		}

		/// <summary>
		/// Từ chối Host Profile (Admin only)
		/// </summary>
		[HttpPost("{id:int}/reject")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> RejectHostProfile(
			int id,
			[FromBody] RejectHostProfileRequest request)
		{
			var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out var adminId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid admin credentials"
				});
			}

			var success = await _hostProfileService.RejectHostProfileAsync(id, adminId, request.Reason);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host profile rejected successfully"
			});
		}

		/// <summary>
		/// Review Host Profile với status tùy chỉnh (Admin only)
		/// </summary>
		[HttpPost("{id:int}/review")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ReviewHostProfile(
			int id,
			[FromBody] ReviewHostProfileRequest request)
		{
			var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out var adminId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid admin credentials"
				});
			}

			var success = await _hostProfileService.ReviewHostProfileAsync(id, adminId, request.Status, request.Note);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host profile reviewed successfully"
			});
		}

		/// <summary>
		/// Đánh dấu/Bỏ đánh dấu Superhost (Admin only)
		/// </summary>
		[HttpPut("{hostProfileId:int}/superhost")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> MarkAsSuperhost(
			int hostProfileId,
			[FromBody] SuperhostRequest request)
		{
			var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out var adminId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid admin credentials"
				});
			}

			var success = await _hostProfileService.MarkAsSuperhostAsync(hostProfileId, adminId, request.IsSuperhost);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = $"Host superhost status set to {(request.IsSuperhost ? "active" : "inactive")} successfully"
			});
		}

		/// <summary>
		/// Bật/Tắt trạng thái hoạt động của Host (Admin only)
		/// </summary>
		[HttpPut("{hostProfileId:int}/active-status")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ToggleActiveStatus(
			int hostProfileId,
			[FromBody] ActiveStatusRequest request)
		{
			var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out var adminId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid admin credentials"
				});
			}

			var success = await _hostProfileService.ToggleActiveStatusAsync(hostProfileId, adminId, request.IsActive);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = $"Host active status set to {(request.IsActive ? "active" : "inactive")} successfully"
			});
		}

		/// <summary>
		/// Xóa Host Profile (Soft delete)
		/// </summary>
		[HttpDelete("{userId:int}")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<ActionResult<ApiResponse<object>>> RemoveHostProfile(int userId)
		{
			var success = await _hostProfileService.RemoveHostProfileAsync(userId);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host profile removed successfully"
			});
		}

		/// <summary>
		/// Cập nhật thống kê Host (System/Admin only - Internal use)
		/// </summary>
		[HttpPut("{hostProfileId:int}/statistics")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateStatistics(
			int hostProfileId,
			[FromBody] UpdateStatisticsRequest request)
		{
			await _hostProfileService.UpdateStatisticsAsync(
				hostProfileId,
				request.TotalHomestays,
				request.TotalBookings,
				request.AverageRating,
				request.ResponseRate,
				request.AverageResponseTime);

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Host statistics updated successfully"
			});
		}
	}

	#region Request Models
	public class ApproveHostProfileRequest
	{
		public string? Note { get; set; }
	}

	public class RejectHostProfileRequest
	{
		public string Reason { get; set; } = string.Empty;
	}

	public class ReviewHostProfileRequest
	{
		public string Status { get; set; } = string.Empty;
		public string? Note { get; set; }
	}

	public class SuperhostRequest
	{
		public bool IsSuperhost { get; set; }
	}

	public class ActiveStatusRequest
	{
		public bool IsActive { get; set; }
	}

	public class UpdateStatisticsRequest
	{
		public int TotalHomestays { get; set; }
		public int TotalBookings { get; set; }
		public decimal AverageRating { get; set; }
		public int ResponseRate { get; set; }
		public TimeSpan? AverageResponseTime { get; set; }
	}
	#endregion
}