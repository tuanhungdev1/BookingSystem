// BookingSystem.Controllers/HomestayController.cs
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Application.DTOs.HomestayDTO;
using BookingSystem.Application.DTOs.HomestayImageDTO;
using BookingSystem.Application.DTOs.HomestayRuleDTO;
using BookingSystem.Application.DTOs.RuleDTO;
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
	public class HomestayController : ControllerBase
	{
		private readonly IHomestayService _homestayService;

		public HomestayController(IHomestayService homestayService)
		{
			_homestayService = homestayService;
		}

		#region Query Endpoints

		/// <summary>
		/// Lấy thông tin homestay theo ID
		/// </summary>
		[HttpGet("{id:int}")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<HomestayDto>>> GetById(int id)
		{
			var homestay = await _homestayService.GetByIdAsync(id);
			if (homestay == null)
			{
				return NotFound(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Homestay not found"
				});
			}

			return Ok(new ApiResponse<HomestayDto>
			{
				Success = true,
				Message = "Homestay retrieved successfully",
				Data = homestay
			});
		}

		/// <summary>
		/// Lấy danh sách homestay có phân trang và filter
		/// </summary>
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<PagedResult<HomestayDto>>>> GetAll([FromQuery] HomestayFilter filter, [FromQuery] int[] propertyTypeIds, [FromQuery] int[] amenityIds)
		{
			if (propertyTypeIds != null && propertyTypeIds.Length > 0)
			{
				filter.PropertyTypeIds = string.Join(",", propertyTypeIds);
			}

			if (amenityIds != null && amenityIds.Length > 0)
			{
				filter.AmenityIds = string.Join(",", amenityIds);
			}

			var homestays = await _homestayService.GetAllHomestayAsync(filter);
			return Ok(new ApiResponse<PagedResult<HomestayDto>>
			{
				Success = true,
				Message = "Homestays retrieved successfully",
				Data = homestays
			});
		}

		#endregion

		#region Command Endpoints - Basic CRUD

		/// <summary>
		/// Tạo mới homestay (Host và Admin)
		/// </summary>
		[HttpPost]
		[Authorize(Policy = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<HomestayDto>>> Create([FromForm] CreateHomestayDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var homestay = await _homestayService.CreateAsync(userId, request);

			return CreatedAtAction(
				nameof(GetById),
				new { id = homestay?.Id },
				new ApiResponse<HomestayDto>
				{
					Success = true,
					Message = "Homestay created successfully. Waiting for admin approval.",
					Data = homestay
				}
			);
		}

		/// <summary>
		/// Cập nhật thông tin homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}")]
		[Authorize(Policy = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<HomestayDto>>> Update(int id, [FromForm] UpdateHomestayDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var homestay = await _homestayService.UpdateAsync(id, userId, request);
			if (homestay == null)
			{
				return NotFound(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Homestay not found"
				});
			}

			return Ok(new ApiResponse<HomestayDto>
			{
				Success = true,
				Message = "Homestay updated successfully",
				Data = homestay
			});
		}

		/// <summary>
		/// Xóa homestay (Admin only)
		/// </summary>
		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
		{
			var success = await _homestayService.DeleteAsync(id);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "Homestay not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Homestay deleted successfully"
			});
		}

		#endregion

		#region Command Endpoints - Status Management

		/// <summary>
		/// Kích hoạt homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/activate")]
		[Authorize(Policy = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
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

			var success = await _homestayService.ActivateAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to activate homestay"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Homestay activated successfully"
			});
		}

		/// <summary>
		/// Vô hiệu hóa homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/deactivate")]
		[Authorize(Roles = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
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

			var success = await _homestayService.DeactivateAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to deactivate homestay"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Homestay deactivated successfully"
			});
		}

		/// <summary>
		/// Thay đổi trạng thái active của homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/status")]
		[Authorize(Roles = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<object>>> SetActiveStatus(int id, [FromBody] bool isActive)
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

			var success = isActive
				? await _homestayService.ActivateAsync(id, userId)
				: await _homestayService.DeactivateAsync(id, userId);

			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Failed to {(isActive ? "activate" : "deactivate")} homestay"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = $"Homestay status set to {(isActive ? "active" : "inactive")} successfully"
			});
		}

		#endregion

		#region Command Endpoints - Image Management

		/// <summary>
		/// Cập nhật hình ảnh homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/images")]
		[Authorize(Roles = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateImages(int id, [FromForm] UpdateHomestayImagesDto request)
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

			var success = await _homestayService.UpdateHomestayImages(id, userId, request);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to update homestay images"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Homestay images updated successfully"
			});
		}

		#endregion

		#region Command Endpoints - Amenities Management

		/// <summary>
		/// Cập nhật danh sách Amenities cho homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/amenities")]
		[Authorize(Roles = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateAmenities(
			int id,
			[FromForm] UpdateHomestayAmenitiesDto amenitieDto)
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

			var success = await _homestayService.UpdateHomestayAmenitiesAsync(id, userId, amenitieDto);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to update homestay amenities"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Homestay amenities updated successfully"
			});
		}

		#endregion

		#region Command Endpoints - Rules Management

		/// <summary>
		/// Cập nhật danh sách Rules cho homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/rules")]
		[Authorize(Roles = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateRules(
			int id,
			[FromForm] UpdateHomestayRulesDto updateDto)
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

			var success = await _homestayService.UpdateHomestayRulesAsync(id, userId, updateDto);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to update homestay rules"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Homestay rules updated successfully"
			});
		}

		#endregion

		/// <summary>
		/// Cập nhật Availability Calendars cho homestay (Thêm mới, Cập nhật, Xóa)
		/// </summary>
		/// <remarks>
		/// Cho phép thực hiện các thao tác:
		/// - NewCalendars: Thêm các ngày mới
		/// - UpdateCalendars: Cập nhật thông tin các ngày hiện có
		/// - DeleteCalendarIds: Xóa các ngày theo ID
		/// </remarks>
		[HttpPut("{id:int}/availability-calendars")]
		[Authorize(Roles = "AdminOrHost")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateAvailabilityCalendars(
			int id,
			[FromBody] UpdateHomestayAvailabilityCalendarsDto request)
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

			var success = await _homestayService.UpdateAvailabilityCalendarsAsync(id, userId, request);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to update availability calendars"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Availability calendars updated successfully"
			});
		}

		#region Command Endpoints - Approval Management

		/// <summary>
		/// Phê duyệt homestay (Admin only)
		/// </summary>
		[HttpPut("{id:int}/approve")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<HomestayDto>>> ApproveHomestay(
			int id,
			[FromBody] ApproveHomestayDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var homestay = await _homestayService.ApproveHomestayAsync(id, userId, request);
			if (homestay == null)
			{
				return NotFound(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Homestay not found"
				});
			}

			return Ok(new ApiResponse<HomestayDto>
			{
				Success = true,
				Message = request.IsApproved
					? "Homestay approved successfully"
					: "Homestay approval status updated",
				Data = homestay
			});
		}

		/// <summary>
		/// Từ chối homestay (Admin only)
		/// </summary>
		[HttpPut("{id:int}/reject")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<HomestayDto>>> RejectHomestay(
			int id,
			[FromBody] string rejectionReason)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var homestay = await _homestayService.RejectHomestayAsync(id, userId, rejectionReason);
			if (homestay == null)
			{
				return NotFound(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Homestay not found"
				});
			}

			return Ok(new ApiResponse<HomestayDto>
			{
				Success = true,
				Message = "Homestay rejected successfully",
				Data = homestay
			});
		}

		/// <summary>
		/// Lấy danh sách homestay chờ phê duyệt (Admin only)
		/// </summary>
		[HttpGet("pending-approvals")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<IEnumerable<HomestayDto>>>> GetPendingApprovals()
		{
			var homestays = await _homestayService.GetPendingApprovalsAsync();
			return Ok(new ApiResponse<IEnumerable<HomestayDto>>
			{
				Success = true,
				Message = "Pending approvals retrieved successfully",
				Data = homestays
			});
		}

		/// <summary>
		/// Đếm số homestay chờ phê duyệt (Admin only)
		/// </summary>
		[HttpGet("pending-approvals/count")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<int>>> CountPendingApprovals()
		{
			var count = await _homestayService.CountPendingApprovalsAsync();
			return Ok(new ApiResponse<int>
			{
				Success = true,
				Message = "Pending approvals count retrieved successfully",
				Data = count
			});
		}

		/// <summary>
		/// Đặt trạng thái Featured cho homestay (Admin only)
		/// </summary>
		[HttpPut("{id:int}/featured")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> SetFeaturedStatus(
			int id,
			[FromBody] bool isFeatured)
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

			var success = await _homestayService.SetFeaturedStatusAsync(id, userId, isFeatured);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to set featured status"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = $"Homestay featured status set to {isFeatured} successfully"
			});
		}

		/// <summary>
		/// Get homestay details by slug
		/// </summary>
		[HttpGet("slug/{slug}")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<HomestayDto>>> GetBySlug(string slug)
		{
			if (string.IsNullOrWhiteSpace(slug))
			{
				return BadRequest(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = "Slug is required"
				});
			}

			var homestay = await _homestayService.GetHomestayBySlugAsync(slug);

			if (homestay == null)
			{
				return NotFound(new ApiResponse<HomestayDto>
				{
					Success = false,
					Message = $"Homestay with slug '{slug}' not found"
				});
			}

			return Ok(new ApiResponse<HomestayDto>
			{
				Success = true,
				Message = "Homestay retrieved successfully",
				Data = homestay
			});
		}

		[HttpGet("my-homestays")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<HomestayDto>>>> GetMyHomestays([FromQuery] HomestayFilter filter)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<PagedResult<HomestayDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var homestays = await _homestayService.GetHomestaysByOwnerIdAsync(userId, filter);

			return Ok(new ApiResponse<PagedResult<HomestayDto>>
			{
				Success = true,
				Message = "Homestays retrieved successfully",
				Data = homestays
			});
		}


		#endregion
	}
}