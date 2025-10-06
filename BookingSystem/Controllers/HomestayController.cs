using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AccommodationDTO.BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.HomestayImageDTO;
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
	[Authorize] // Require authentication for all endpoints
	public class HomestayController : ControllerBase
	{
		private readonly IHomestayService _homestayService;

		public HomestayController(IHomestayService homestayService)
		{
			_homestayService = homestayService;
		}

		/// <summary>
		/// Lấy thông tin homestay theo ID
		/// </summary>
		[HttpGet("{id:int}")]
		[AllowAnonymous] // Cho phép guest xem thông tin homestay
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
		[AllowAnonymous] // Cho phép guest xem danh sách homestay
		public async Task<ActionResult<ApiResponse<PagedResult<HomestayDto>>>> GetAll([FromQuery] HomestayFilter filter)
		{
			var homestays = await _homestayService.GetAllHomestayAsync(filter);
			return Ok(new ApiResponse<PagedResult<HomestayDto>>
			{
				Success = true,
				Message = "Homestays retrieved successfully",
				Data = homestays
			});
		}

		/// <summary>
		/// Tạo mới homestay (Host và Admin)
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<HomestayDto>>> Create([FromForm] CreateHomestayDto request)
		{
			// Lấy UserId từ Claims
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
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<HomestayDto>>> Update(int id, [FromForm] UpdateHomestayDto request)
		{
			// Lấy UserId từ Claims
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
		/// Cập nhật hình ảnh homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/images")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateImages(int id, [FromForm] UpdateHomestayImagesDto request)
		{
			// Lấy UserId từ Claims
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

		/// <summary>
		/// Kích hoạt homestay (Owner hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/activate")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
		{
			// Lấy UserId từ Claims
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
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
		{
			// Lấy UserId từ Claims
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
		/// Alternative endpoint để toggle status
		/// </summary>
		[HttpPut("{id:int}/status")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> SetActiveStatus(int id, [FromBody] bool isActive)
		{
			// Lấy UserId từ Claims
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
	}
}