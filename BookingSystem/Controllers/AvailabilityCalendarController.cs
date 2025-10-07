using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
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
	public class AvailabilityCalendarController : ControllerBase
	{
		private readonly IAvailabilityCalendarService _availabilityCalendarService;

		public AvailabilityCalendarController(IAvailabilityCalendarService availabilityCalendarService)
		{
			_availabilityCalendarService = availabilityCalendarService;
		}

		// Helper method to get current user ID
		private int GetCurrentUserId()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			return int.Parse(userId);
		}

		// ---------------------- GET METHODS ----------------------

		/// <summary>
		/// Lấy thông tin lịch khả dụng theo ID
		/// </summary>
		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<AvailabilityCalendarDto>>> GetById(int id)
		{
			var calendar = await _availabilityCalendarService.GetByIdAsync(id);
			if (calendar == null)
			{
				return NotFound(new ApiResponse<AvailabilityCalendarDto>
				{
					Success = false,
					Message = "Availability calendar not found"
				});
			}

			return Ok(new ApiResponse<AvailabilityCalendarDto>
			{
				Success = true,
				Message = "Availability calendar retrieved successfully",
				Data = calendar
			});
		}

		/// <summary>
		/// Lấy lịch khả dụng theo homestay và ngày cụ thể
		/// </summary>
		[HttpGet("by-homestay")]
		public async Task<ActionResult<ApiResponse<AvailabilityCalendarDto>>> GetByHomestayAndDate(
			[FromQuery] int homestayId, [FromQuery] DateOnly date)
		{
			var calendar = await _availabilityCalendarService.GetByHomestayAndDateAsync(homestayId, date);
			return Ok(new ApiResponse<AvailabilityCalendarDto>
			{
				Success = true,
				Message = "Availability calendar retrieved successfully",
				Data = calendar
			});
		}

		/// <summary>
		/// Lấy danh sách lịch khả dụng trong khoảng ngày
		/// </summary>
		[HttpGet("range")]
		public async Task<ActionResult<ApiResponse<IEnumerable<AvailabilityCalendarDto>>>> GetByDateRange(
			[FromQuery] int homestayId,
			[FromQuery] DateOnly startDate,
			[FromQuery] DateOnly endDate)
		{
			var result = await _availabilityCalendarService.GetByDateRangeAsync(homestayId, startDate, endDate);
			return Ok(new ApiResponse<IEnumerable<AvailabilityCalendarDto>>
			{
				Success = true,
				Message = "Availability calendars retrieved successfully",
				Data = result
			});
		}

		/// <summary>
		/// Lấy lịch theo tháng cụ thể
		/// </summary>
		[HttpGet("month")]
		public async Task<ActionResult<ApiResponse<CalendarMonthDto>>> GetMonthCalendar(
			[FromQuery] int homestayId, [FromQuery] int year, [FromQuery] int month)
		{
			var result = await _availabilityCalendarService.GetMonthCalendarAsync(homestayId, year, month);
			return Ok(new ApiResponse<CalendarMonthDto>
			{
				Success = true,
				Message = "Month calendar retrieved successfully",
				Data = result
			});
		}

		/// <summary>
		/// Kiểm tra tình trạng khả dụng của homestay trong khoảng ngày
		/// </summary>
		[HttpGet("check")]
		public async Task<ActionResult<ApiResponse<DateRangeAvailabilityDto>>> CheckAvailability(
			[FromQuery] int homestayId,
			[FromQuery] DateOnly startDate,
			[FromQuery] DateOnly endDate)
		{
			var result = await _availabilityCalendarService.CheckDateRangeAvailabilityAsync(homestayId, startDate, endDate);
			return Ok(new ApiResponse<DateRangeAvailabilityDto>
			{
				Success = true,
				Message = "Availability checked successfully",
				Data = result
			});
		}

		/// <summary>
		/// Lấy danh sách có filter (phân trang, lọc)
		/// </summary>
		[HttpGet("filter")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<AvailabilityCalendarDto>>>> GetWithFilter([FromQuery] AvailabilityCalendarFilter filter)
		{
			var result = await _availabilityCalendarService.GetCalendarWithFilterAsync(filter);
			return Ok(new ApiResponse<PagedResult<AvailabilityCalendarDto>>
			{
				Success = true,
				Message = "Filtered availability calendars retrieved successfully",
				Data = result
			});
		}

		// ---------------------- CREATE METHODS ----------------------

		/// <summary>
		/// Tạo mới 1 lịch khả dụng
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<AvailabilityCalendarDto>>> Create([FromBody] CreateAvailabilityCalendarDto request)
		{
			var ownerId = GetCurrentUserId();
			var result = await _availabilityCalendarService.CreateAsync(ownerId, request);
			return Ok(new ApiResponse<AvailabilityCalendarDto>
			{
				Success = true,
				Message = "Availability calendar created successfully",
				Data = result
			});
		}

		/// <summary>
		/// Tạo hàng loạt lịch khả dụng
		/// </summary>
		[HttpPost("bulk")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<List<AvailabilityCalendarDto>>>> BulkCreate([FromBody] BulkCreateAvailabilityCalendarDto request)
		{
			var ownerId = GetCurrentUserId();
			var result = await _availabilityCalendarService.BulkCreateAsync(ownerId, request);
			return Ok(new ApiResponse<List<AvailabilityCalendarDto>>
			{
				Success = true,
				Message = "Bulk availability calendars created successfully",
				Data = result
			});
		}

		// ---------------------- UPDATE METHODS ----------------------

		/// <summary>
		/// Cập nhật 1 lịch khả dụng
		/// </summary>
		[HttpPut("{id:int}")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<AvailabilityCalendarDto>>> Update(int id, [FromBody] UpdateAvailabilityCalendarDto request)
		{
			var ownerId = GetCurrentUserId();
			var result = await _availabilityCalendarService.UpdateAsync(ownerId, id, request);
			return Ok(new ApiResponse<AvailabilityCalendarDto>
			{
				Success = true,
				Message = "Availability calendar updated successfully",
				Data = result
			});
		}

		/// <summary>
		/// Cập nhật hàng loạt lịch khả dụng
		/// </summary>
		[HttpPut("bulk/{homestayId:int}")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> BulkUpdate(int homestayId, [FromBody] BulkUpdateAvailabilityCalendarDto request)
		{
			var ownerId = GetCurrentUserId();
			var success = await _availabilityCalendarService.BulkUpdateAsync(ownerId, homestayId, request);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = "Bulk availability calendars updated successfully"
			});
		}

		// ---------------------- DELETE METHODS ----------------------

		/// <summary>
		/// Xóa 1 lịch khả dụng
		/// </summary>
		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
		{
			var ownerId = GetCurrentUserId();
			var success = await _availabilityCalendarService.DeleteAsync(ownerId, id);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = "Availability calendar deleted successfully"
			});
		}

		/// <summary>
		/// Xóa các lịch trong khoảng ngày
		/// </summary>
		[HttpDelete("range/{homestayId:int}")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> DeleteByDateRange(
			int homestayId, AvailabilityRangeRequest request)
		{
			var ownerId = GetCurrentUserId();
			var success = await _availabilityCalendarService.DeleteByDateRangeAsync(ownerId, homestayId, request.StartDate, request.EndDate);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = "Availability calendars deleted successfully"
			});
		}

		// ---------------------- BLOCK / UNBLOCK ----------------------

		/// <summary>
		/// Chặn (block) các ngày không khả dụng
		/// </summary>
		[HttpPost("block/{homestayId:int}")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> BlockDates(int homestayId, [FromBody] BlockDatesDto request)
		{
			var ownerId = GetCurrentUserId();
			var success = await _availabilityCalendarService.BlockDatesAsync(ownerId, homestayId, request);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = "Dates blocked successfully"
			});
		}

		/// <summary>
		/// Bỏ chặn (unblock) các ngày
		/// </summary>
		[HttpPost("unblock/{homestayId:int}")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UnblockDates(
			int homestayId, AvailabilityRangeRequest request)
		{
			var ownerId = GetCurrentUserId();
			var success = await _availabilityCalendarService.UnblockDatesAsync(ownerId, homestayId, request.StartDate, request.EndDate);
			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = "Dates unblocked successfully"
			});
		}
	}
}
