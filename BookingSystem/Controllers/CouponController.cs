using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.CouponDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class CouponController : ControllerBase
	{
		private readonly ICouponService _couponService;

		public CouponController(ICouponService couponService)
		{
			_couponService = couponService;
		}

		#region Homestay Applicable Coupons

		/// <summary>
		/// [Public] Lấy danh sách coupon active áp dụng cho homestay (cho guest xem)
		/// </summary>
		[HttpGet("homestay/{homestayId:int}/active")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<IEnumerable<CouponDto>>>> GetActiveApplicableCouponsForHomestay(
			int homestayId,
			[FromQuery] decimal? bookingAmount = null,
			[FromQuery] int? numberOfNights = null)
		{
			try
			{
				// Lấy userId nếu user đã login
				int? userId = null;
				if (User.Identity?.IsAuthenticated == true)
				{
					var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedUserId))
					{
						userId = parsedUserId;
					}
				}

				var coupons = await _couponService.GetActiveApplicableCouponsForHomestayAsync(
					homestayId, userId, bookingAmount, numberOfNights);

				return Ok(new ApiResponse<IEnumerable<CouponDto>>
				{
					Success = true,
					Message = "Coupons retrieved successfully.",
					Data = coupons
				});
			}
			catch (NotFoundException ex)
			{
				return NotFound(new ApiResponse<IEnumerable<CouponDto>>
				{
					Success = false,
					Message = ex.Message
				});
			}
			catch (BadRequestException ex)
			{
				return BadRequest(new ApiResponse<IEnumerable<CouponDto>>
				{
					Success = false,
					Message = ex.Message
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<IEnumerable<CouponDto>>
				{
					Success = false,
					Message = "Internal server error."
				});
			}
		}

		/// <summary>
		/// [Host/Admin] Lấy TẤT CẢ coupon áp dụng cho homestay (bao gồm inactive)
		/// </summary>
		[HttpGet("homestay/{homestayId:int}/all")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<IEnumerable<CouponDto>>>> GetApplicableCouponsForHomestay(
			int homestayId,
			[FromQuery] bool includeInactive = false)
		{
			try
			{
				var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
				{
					return Unauthorized(new ApiResponse<IEnumerable<CouponDto>>
					{
						Success = false,
						Message = "Invalid user authentication"
					});
				}

				var coupons = await _couponService.GetApplicableCouponsForHomestayAsync(
					homestayId, includeInactive);

				return Ok(new ApiResponse<IEnumerable<CouponDto>>
				{
					Success = true,
					Message = "Coupons retrieved successfully.",
					Data = coupons
				});
			}
			catch (NotFoundException ex)
			{
				return NotFound(new ApiResponse<IEnumerable<CouponDto>>
				{
					Success = false,
					Message = ex.Message
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<IEnumerable<CouponDto>>
				{
					Success = false,
					Message = "Internal server error."
				});
			}
		}

		#endregion


		#region CRUD Operations

		/// <summary>
		/// Tạo coupon mới (Admin hoặc Host)
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Admin,Host")]
		public async Task<ActionResult<ApiResponse<CouponDto>>> Create([FromBody] CreateCouponDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<CouponDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var coupon = await _couponService.CreateCouponAsync(userId, request);

			return CreatedAtAction(
				nameof(GetById),
				new { id = coupon?.Id },
				new ApiResponse<CouponDto>
				{
					Success = true,
					Message = "Coupon created successfully",
					Data = coupon
				}
			);
		}

		/// <summary>
		/// Lấy thông tin coupon theo ID
		/// </summary>
		[HttpGet("{id:int}")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<ActionResult<ApiResponse<CouponDto>>> GetById(int id)
		{
			var coupon = await _couponService.GetByIdAsync(id);
			if (coupon == null)
			{
				return NotFound(new ApiResponse<CouponDto>
				{
					Success = false,
					Message = "Coupon not found"
				});
			}

			return Ok(new ApiResponse<CouponDto>
			{
				Success = true,
				Message = "Coupon retrieved successfully",
				Data = coupon
			});
		}

		/// <summary>
		/// Lấy thông tin coupon theo code (Public cho guest check)
		/// </summary>
		[HttpGet("code/{couponCode}")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<CouponDto>>> GetByCode(string couponCode)
		{
			var coupon = await _couponService.GetByCodeAsync(couponCode);
			if (coupon == null)
			{
				return NotFound(new ApiResponse<CouponDto>
				{
					Success = false,
					Message = "Coupon not found"
				});
			}

			return Ok(new ApiResponse<CouponDto>
			{
				Success = true,
				Message = "Coupon retrieved successfully",
				Data = coupon
			});
		}

		/// <summary>
		/// Lấy tất cả coupons (Admin only)
		/// </summary>
		[HttpGet("all")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<CouponDto>>>> GetAllCoupons(
			[FromQuery] CouponFilter filter)
		{
			var coupons = await _couponService.GetAllCouponsAsync(filter);

			return Ok(new ApiResponse<PagedResult<CouponDto>>
			{
				Success = true,
				Message = "Coupons retrieved successfully",
				Data = coupons
			});
		}

		/// <summary>
		/// Lấy danh sách coupons của host (Host xem coupons của mình)
		/// </summary>
		[HttpGet("my-coupons")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<CouponDto>>>> GetMyCoupons(
			[FromQuery] CouponFilter filter)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<PagedResult<CouponDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
			if (!roles.Contains("Admin"))
			{
				filter.CreatedByUserId = userId;
			}

			var coupons = await _couponService.GetAllCouponsAsync(filter);

			return Ok(new ApiResponse<PagedResult<CouponDto>>
			{
				Success = true,
				Message = "Your coupons retrieved successfully",
				Data = coupons
			});
		}

		/// <summary>
		/// Lấy danh sách coupons công khai (Cho guest xem)
		/// </summary>
		[HttpGet("public")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<PagedResult<CouponDto>>>> GetPublicCoupons(
			[FromQuery] CouponFilter filter)
		{
			var coupons = await _couponService.GetPublicCouponsAsync(filter);

			return Ok(new ApiResponse<PagedResult<CouponDto>>
			{
				Success = true,
				Message = "Public coupons retrieved successfully",
				Data = coupons
			});
		}

		/// <summary>
		/// Cập nhật coupon (Admin hoặc Host owner)
		/// </summary>
		[HttpPut("{id:int}")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<ActionResult<ApiResponse<CouponDto>>> Update(
			int id,
			[FromBody] UpdateCouponDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<CouponDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var coupon = await _couponService.UpdateCouponAsync(id, userId, request);
			if (coupon == null)
			{
				return NotFound(new ApiResponse<CouponDto>
				{
					Success = false,
					Message = "Coupon not found"
				});
			}

			return Ok(new ApiResponse<CouponDto>
			{
				Success = true,
				Message = "Coupon updated successfully",
				Data = coupon
			});
		}

		/// <summary>
		/// Xóa coupon (Admin hoặc Host owner)
		/// </summary>
		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin,Host")]
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

			var success = await _couponService.DeleteCouponAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to delete coupon"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Coupon deleted successfully"
			});
		}

		#endregion

		#region Coupon Validation & Application

		/// <summary>
		/// Lấy coupons available cho user và booking cụ thể
		/// </summary>
		[HttpGet("available")]
		public async Task<ActionResult<ApiResponse<IEnumerable<CouponDto>>>> GetAvailableCoupons(
			[FromQuery] int homestayId,
			[FromQuery] decimal bookingAmount,
			[FromQuery] int nights)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<IEnumerable<CouponDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var coupons = await _couponService.GetAvailableCouponsForUserAsync(
				userId, homestayId, bookingAmount, nights);

			return Ok(new ApiResponse<IEnumerable<CouponDto>>
			{
				Success = true,
				Message = "Available coupons retrieved successfully",
				Data = coupons
			});
		}

		/// <summary>
		/// Validate coupon trước khi apply
		/// </summary>
		[HttpPost("validate")]
		public async Task<ActionResult<ApiResponse<CouponValidationResultDto>>> ValidateCoupon(
			[FromBody] ValidateCouponDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<CouponValidationResultDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var result = await _couponService.ValidateCouponAsync(userId, request);

			return Ok(new ApiResponse<CouponValidationResultDto>
			{
				Success = result.IsValid,
				Message = result.Message,
				Data = result
			});
		}

		/// <summary>
		/// Calculate discount amount
		/// </summary>
		[HttpGet("calculate-discount")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<decimal>>> CalculateDiscount(
			[FromQuery] string couponCode,
			[FromQuery] decimal bookingAmount,
			[FromQuery] int nights)
		{
			var discountAmount = await _couponService.CalculateDiscountAmountAsync(
				couponCode, bookingAmount, nights);

			return Ok(new ApiResponse<decimal>
			{
				Success = true,
				Message = "Discount calculated successfully",
				Data = discountAmount
			});
		}

		#endregion

		#region Coupon Usage

		/// <summary>
		/// Apply coupon to booking
		/// </summary>
		[HttpPost("apply")]
		[Authorize(Roles = "Guest,Admin")]
		public async Task<ActionResult<ApiResponse<CouponUsageDto>>> ApplyCouponToBooking(
			[FromQuery] int bookingId,
			[FromQuery] string couponCode)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<CouponUsageDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var usage = await _couponService.ApplyCouponToBookingAsync(bookingId, couponCode, userId);

			return Ok(new ApiResponse<CouponUsageDto>
			{
				Success = true,
				Message = "Coupon applied successfully",
				Data = usage
			});
		}

		/// <summary>
		/// Remove coupon from booking
		/// </summary>
		[HttpDelete("remove")]
		//[Authorize(Roles = "Guest,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> RemoveCouponFromBooking([FromQuery] int bookingId, [FromBody] RemoveCouponDto request)
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

			var success = await _couponService.RemoveCouponFromBookingAsync(bookingId, userId, request.CouponCode);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to remove coupon"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Coupon removed successfully"
			});
		}

		/// <summary>
		/// Get all coupon usages by booking ID (hỗ trợ nhiều coupon)
		/// </summary>
		[HttpGet("booking/{bookingId:int}/usages")]
		public async Task<ActionResult<ApiResponse<IEnumerable<CouponUsageDto>>>> GetCouponUsagesByBooking(int bookingId)
		{
			var usages = await _couponService.GetCouponUsagesByBookingAsync(bookingId);

			return Ok(new ApiResponse<IEnumerable<CouponUsageDto>>
			{
				Success = true,
				Message = "Coupon usages retrieved successfully",
				Data = usages
			});
		}

		/// <summary>
		/// Get coupon usage history của user hiện tại
		/// </summary>
		[HttpGet("usage/my-history")]
		public async Task<ActionResult<ApiResponse<IEnumerable<CouponUsageDto>>>> GetMyUsageHistory()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<IEnumerable<CouponUsageDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var history = await _couponService.GetUserCouponUsageHistoryAsync(userId);

			return Ok(new ApiResponse<IEnumerable<CouponUsageDto>>
			{
				Success = true,
				Message = "Usage history retrieved successfully",
				Data = history
			});
		}

		/// <summary>
		/// Get coupon usage history by coupon ID (Admin, Host owner)
		/// </summary>
		[HttpGet("{id:int}/usage-history")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<ActionResult<ApiResponse<IEnumerable<CouponUsageDto>>>> GetCouponUsageHistory(int id)
		{
			var history = await _couponService.GetCouponUsageHistoryAsync(id);

			return Ok(new ApiResponse<IEnumerable<CouponUsageDto>>
			{
				Success = true,
				Message = "Coupon usage history retrieved successfully",
				Data = history
			});
		}

		#endregion

		#region Status Management


		/// <summary>
		/// Activate coupon (Admin hoặc Host owner)
		/// </summary>
		[HttpPut("{id:int}/activate")]
		[Authorize(Roles = "Admin,Host")]
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

			var success = await _couponService.ActivateCouponAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to activate coupon"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Coupon activated successfully"
			});
		}

		/// <summary>
		/// Deactivate coupon (Admin hoặc Host owner)
		/// </summary>
		[HttpPut("{id:int}/deactivate")]
		[Authorize(Roles = "Admin,Host")]
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

			var success = await _couponService.DeactivateCouponAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to deactivate coupon"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Coupon deactivated successfully"
			});
		}

		/// <summary>
		/// Toggle coupon status (Admin hoặc Host owner)
		/// </summary>
		[HttpPut("{id:int}/toggle-status")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<ActionResult<ApiResponse<object>>> ToggleStatus(int id)
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

			// Get current status
			var coupon = await _couponService.GetByIdAsync(id);
			if (coupon == null)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "Coupon not found"
				});
			}

			// Toggle
			var success = coupon.IsActive
				? await _couponService.DeactivateCouponAsync(id, userId)
				: await _couponService.ActivateCouponAsync(id, userId);

			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to toggle coupon status"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = $"Coupon {(coupon.IsActive ? "deactivated" : "activated")} successfully"
			});
		}

		/// <summary>
		/// Extend coupon expiry date (Admin hoặc Host owner)
		/// </summary>
		[HttpPut("{id:int}/extend")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<ActionResult<ApiResponse<object>>> ExtendExpiry(
			int id,
			[FromBody] DateTime newEndDate)
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

			var success = await _couponService.ExtendCouponExpiryAsync(id, userId, newEndDate);

			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to extend coupon expiry"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Coupon expiry extended successfully"
			});
		}

		#endregion

		#region Statistics & Reports

		/// <summary>
		/// Get coupon statistics (Admin xem tất cả, Host xem của mình)
		/// </summary>
		[HttpGet("statistics")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<ActionResult<ApiResponse<CouponStatisticsDto>>> GetStatistics()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<CouponStatisticsDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
			var isAdmin = roles.Contains("Admin");

			// Admin xem tất cả, Host chỉ xem của mình
			int? createdByUserId = isAdmin ? null : userId;
			var statistics = await _couponService.GetCouponStatisticsAsync(createdByUserId);

			return Ok(new ApiResponse<CouponStatisticsDto>
			{
				Success = true,
				Message = "Statistics retrieved successfully",
				Data = statistics
			});
		}

		/// <summary>
		/// Process expired coupons (Background job - Admin only)
		/// </summary>
		[HttpPost("process-expired")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ProcessExpiredCoupons()
		{
			await _couponService.ProcessExpiredCouponsAsync();

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Expired coupons processed successfully"
			});
		}

		#endregion
	}
}