using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.BookingDTO;
using BookingSystem.Application.DTOs.ExportDTOs;
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
	public class BookingController : ControllerBase
	{
		private readonly IBookingService _bookingService;
		private readonly IGenericExportService _exportService;

		public BookingController(IBookingService bookingService, IGenericExportService exportService)
		{
			_bookingService = bookingService;
			_exportService = exportService;
		}

		[HttpGet("export/excel")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<IActionResult> ExportBookingsToExcel([FromQuery] BookingFilter filter)
		{
			try
			{
				var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
					return Unauthorized();

				var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
				var isAdmin = roles.Contains("Admin");

				PagedResult<BookingDto> bookings;
				if (isAdmin)
					bookings = await _bookingService.GetAllBookingsAsync(filter);
				else
					bookings = await _bookingService.GetHostBookingsAsync(userId, filter);

				var exportData = bookings.Items.Select(b => new BookingExportDto
				{
					Id = b.Id,
					BookingCode = b.BookingCode,
					GuestId = b.GuestId,
					GuestFullName = b.GuestFullName,
					GuestEmail = b.GuestEmail,
					GuestPhoneNumber = b.GuestPhoneNumber,
					HomestayId = b.Homestay.Id,
					HomestayTitle = b.Homestay?.HomestayTitle ?? "",
					CheckInDate = b.CheckInDate,
					CheckOutDate = b.CheckOutDate,
					NumberOfGuests = b.NumberOfGuests,
					NumberOfAdults = b.NumberOfAdults,
					NumberOfChildren = b.NumberOfChildren,
					BaseAmount = b.BaseAmount,
					CleaningFee = b.CleaningFee,
					ServiceFee = b.ServiceFee,
					TaxAmount = b.TaxAmount,
					DiscountAmount = b.DiscountAmount,
					TotalAmount = b.TotalAmount,
					BookingStatus = b.BookingStatus.ToString(),
					IsBookingForSomeoneElse = b.IsBookingForSomeoneElse,
					ActualGuestFullName = b.ActualGuestFullName,
					ActualGuestPhoneNumber = b.ActualGuestPhoneNumber,
					CreatedAt = b.CreatedAt,
					CancelledAt = b.CancelledAt
				});

				var excelData = await _exportService.ExportToExcelAsync(
					exportData,
					sheetName: "Bookings",
					fileName: "bookings.xlsx"
				);

				return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "bookings.xlsx");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Failed to export: {ex.Message}"
				});
			}
		}

		[HttpGet("export/pdf")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<IActionResult> ExportBookingsToPdf([FromQuery] BookingFilter filter)
		{
			try
			{
				var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
					return Unauthorized();

				var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
				var isAdmin = roles.Contains("Admin");

				PagedResult<BookingDto> bookings;
				if (isAdmin)
					bookings = await _bookingService.GetAllBookingsAsync(filter);
				else
					bookings = await _bookingService.GetHostBookingsAsync(userId, filter);

				var exportData = bookings.Items.Select(b => new BookingExportDto
				{
					Id = b.Id,
					BookingCode = b.BookingCode,
					GuestId = b.GuestId,
					GuestFullName = b.GuestFullName,
					GuestEmail = b.GuestEmail,
					GuestPhoneNumber = b.GuestPhoneNumber,
					HomestayId = b.Homestay.Id,
					HomestayTitle = b.Homestay?.HomestayTitle ?? "",
					CheckInDate = b.CheckInDate,
					CheckOutDate = b.CheckOutDate,
					NumberOfGuests = b.NumberOfGuests,
					NumberOfAdults = b.NumberOfAdults,
					NumberOfChildren = b.NumberOfChildren,
					BaseAmount = b.BaseAmount,
					CleaningFee = b.CleaningFee,
					ServiceFee = b.ServiceFee,
					TaxAmount = b.TaxAmount,
					DiscountAmount = b.DiscountAmount,
					TotalAmount = b.TotalAmount,
					BookingStatus = b.BookingStatus.ToString(),
					IsBookingForSomeoneElse = b.IsBookingForSomeoneElse,
					ActualGuestFullName = b.ActualGuestFullName,
					ActualGuestPhoneNumber = b.ActualGuestPhoneNumber,
					CreatedAt = b.CreatedAt,
					CancelledAt = b.CancelledAt
				});

				var pdfData = await _exportService.ExportToPdfAsync(
					exportData,
					title: "BOOKINGS REPORT",
					fileName: "bookings.pdf"
				);

				return File(pdfData, "application/pdf", "bookings.pdf");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Failed to export: {ex.Message}"
				});
			}
		}

		[HttpGet("export/csv")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<IActionResult> ExportBookingsToCSV([FromQuery] BookingFilter filter)
		{
			try
			{
				var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (!int.TryParse(userIdClaim, out int userId))
					return Unauthorized();

				var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
				var isAdmin = roles.Contains("Admin");

				PagedResult<BookingDto> bookings;
				if (isAdmin)
					bookings = await _bookingService.GetAllBookingsAsync(filter);
				else
					bookings = await _bookingService.GetHostBookingsAsync(userId, filter);

				var exportData = bookings.Items.Select(b => new BookingExportDto
				{
					Id = b.Id,
					BookingCode = b.BookingCode,
					GuestId = b.GuestId,
					GuestFullName = b.GuestFullName,
					GuestEmail = b.GuestEmail,
					GuestPhoneNumber = b.GuestPhoneNumber,
					HomestayId = b.Homestay.Id,
					HomestayTitle = b.Homestay?.HomestayTitle ?? "",
					CheckInDate = b.CheckInDate,
					CheckOutDate = b.CheckOutDate,
					NumberOfGuests = b.NumberOfGuests,
					NumberOfAdults = b.NumberOfAdults,
					NumberOfChildren = b.NumberOfChildren,
					BaseAmount = b.BaseAmount,
					CleaningFee = b.CleaningFee,
					ServiceFee = b.ServiceFee,
					TaxAmount = b.TaxAmount,
					DiscountAmount = b.DiscountAmount,
					TotalAmount = b.TotalAmount,
					BookingStatus = b.BookingStatus.ToString(),
					IsBookingForSomeoneElse = b.IsBookingForSomeoneElse,
					ActualGuestFullName = b.ActualGuestFullName,
					ActualGuestPhoneNumber = b.ActualGuestPhoneNumber,
					CreatedAt = b.CreatedAt,
					CancelledAt = b.CancelledAt
				});

				var csvData = await _exportService.ExportToCSVAsync(
					exportData,
					fileName: "bookings.csv"
				);

				return File(csvData, "text/csv", "bookings.csv");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Failed to export: {ex.Message}"
				});
			}
		}

		/// <summary>
		/// Tính toán giá booking trước khi đặt
		/// </summary>
		[HttpPost("calculate-price")]
		[AllowAnonymous] // Cho phép guest xem giá trước khi đăng nhập
		public async Task<ActionResult<ApiResponse<BookingPriceBreakdownDto>>> CalculatePrice(
			[FromBody] BookingPriceCalculationDto request)
		{
			var priceBreakdown = await _bookingService.CalculateBookingPriceAsync(request);

			return Ok(new ApiResponse<BookingPriceBreakdownDto>
			{
				Success = true,
				Message = "Price calculated successfully",
				Data = priceBreakdown
			});
		}

		/// <summary>
		/// Kiểm tra homestay có available không
		/// </summary>
		[HttpGet("check-availability")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<bool>>> CheckAvailability(
			[FromQuery] int homestayId,
			[FromQuery] DateTime checkInDate,
			[FromQuery] DateTime checkOutDate,
			[FromQuery] int? excludeBookingId = null)
		{
			var isAvailable = await _bookingService.IsHomestayAvailableAsync(
				homestayId, checkInDate, checkOutDate, excludeBookingId);

			return Ok(new ApiResponse<bool>
			{
				Success = true,
				Message = isAvailable ? "Homestay is available" : "Homestay is not available",
				Data = isAvailable
			});
		}

		/// <summary>
		/// Tạo booking mới (Guest)
		/// </summary>
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<ApiResponse<BookingDto>>> Create([FromBody] CreateBookingDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<BookingDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var booking = await _bookingService.CreateBookingAsync(userId, request);

			return CreatedAtAction(
				nameof(GetById),
				new { id = booking?.Id },
				new ApiResponse<BookingDto>
				{
					Success = true,
					Message = "Booking created successfully. Waiting for host confirmation.",
					Data = booking
				}
			);
		}

		/// <summary>
		/// Lấy thông tin booking theo ID
		/// </summary>
		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<BookingDto>>> GetById(int id)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<BookingDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var booking = await _bookingService.GetByIdAsync(id, userId);
			if (booking == null)
			{
				return NotFound(new ApiResponse<BookingDto>
				{
					Success = false,
					Message = "Booking not found"
				});
			}

			return Ok(new ApiResponse<BookingDto>
			{
				Success = true,
				Message = "Booking retrieved successfully",
				Data = booking
			});
		}

		/// <summary>
		/// Lấy thông tin booking theo booking code
		/// </summary>
		[HttpGet("code/{bookingCode}")]
		public async Task<ActionResult<ApiResponse<BookingDto>>> GetByBookingCode(string bookingCode)
		{
			var booking = await _bookingService.GetByBookingCodeAsync(bookingCode);
			if (booking == null)
			{
				return NotFound(new ApiResponse<BookingDto>
				{
					Success = false,
					Message = "Booking not found"
				});
			}

			return Ok(new ApiResponse<BookingDto>
			{
				Success = true,
				Message = "Booking retrieved successfully",
				Data = booking
			});
		}

		/// <summary>
		/// Lấy danh sách booking của user (Guest xem booking của mình)
		/// </summary>
		[HttpGet("my-bookings")]
		[Authorize]
		public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetMyBookings(
			[FromQuery] BookingFilter filter)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<PagedResult<BookingDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var bookings = await _bookingService.GetUserBookingsAsync(userId, filter);

			return Ok(new ApiResponse<PagedResult<BookingDto>>
			{
				Success = true,
				Message = "Bookings retrieved successfully",
				Data = bookings
			});
		}

		/// <summary>
		/// Lấy danh sách booking của host (Host xem booking homestay của mình)
		/// </summary>
		[HttpGet("host-bookings")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetHostBookings(
			[FromQuery] BookingFilter filter)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<PagedResult<BookingDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var bookings = await _bookingService.GetHostBookingsAsync(userId, filter);

			return Ok(new ApiResponse<PagedResult<BookingDto>>
			{
				Success = true,
				Message = "Host bookings retrieved successfully",
				Data = bookings
			});
		}

		/// <summary>
		/// Lấy tất cả booking (Admin only)
		/// </summary>
		[HttpGet("all")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetAllBookings(
			[FromQuery] BookingFilter filter)
		{
			var bookings = await _bookingService.GetAllBookingsAsync(filter);

			return Ok(new ApiResponse<PagedResult<BookingDto>>
			{
				Success = true,
				Message = "All bookings retrieved successfully",
				Data = bookings
			});
		}

		/// <summary>
		/// Cập nhật booking (Guest, Host hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}")]
		public async Task<ActionResult<ApiResponse<BookingDto>>> Update(
			int id,
			[FromBody] UpdateBookingDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<BookingDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var booking = await _bookingService.UpdateBookingAsync(id, userId, request);
			if (booking == null)
			{
				return NotFound(new ApiResponse<BookingDto>
				{
					Success = false,
					Message = "Booking not found"
				});
			}

			return Ok(new ApiResponse<BookingDto>
			{
				Success = true,
				Message = "Booking updated successfully",
				Data = booking
			});
		}

		/// <summary>
		/// Xác nhận booking (Host hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/confirm")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ConfirmBooking(int id)
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

			var success = await _bookingService.ConfirmBookingAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to confirm booking"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Booking confirmed successfully"
			});
		}

		/// <summary>
		/// Từ chối booking (Host hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/reject")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> RejectBooking(
			int id,
			[FromBody] string reason)
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

			var success = await _bookingService.RejectBookingAsync(id, userId, reason);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to reject booking"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Booking rejected successfully"
			});
		}

		/// <summary>
		/// Hủy booking (Guest hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/cancel")]
		[Authorize(Roles = "Guest,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> CancelBooking(
			int id,
			[FromBody] CancelBookingDto request)
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

			var success = await _bookingService.CancelBookingAsync(id, userId, request);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to cancel booking"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Booking cancelled successfully"
			});
		}

		/// <summary>
		/// Check-in (Host hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/check-in")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> CheckIn(int id)
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

			var success = await _bookingService.CheckInAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to check in booking"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Booking checked in successfully"
			});
		}

		/// <summary>
		/// Check-out (Host hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/check-out")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> CheckOut(int id)
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

			var success = await _bookingService.CheckOutAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to check out booking"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Booking checked out successfully"
			});
		}

		/// <summary>
		/// Đánh dấu booking là hoàn thành (System/Admin)
		/// </summary>
		[HttpPut("{id:int}/complete")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> MarkAsCompleted(int id)
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
			var success = await _bookingService.MarkAsCompletedAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to mark booking as completed"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Booking marked as completed successfully"
			});
		}

		/// <summary>
		/// Đánh dấu booking là no-show (Host hoặc Admin)
		/// </summary>
		[HttpPut("{id:int}/no-show")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> MarkAsNoShow(int id)
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

			var success = await _bookingService.MarkAsNoShowAsync(id, userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to mark booking as no-show"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Booking marked as no-show successfully"
			});
		}

		/// <summary>
		/// Lấy thống kê booking (Host xem của mình, Admin xem tất cả)
		/// </summary>
		[HttpGet("statistics")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<BookingStatisticsDto>>> GetStatistics(
			[FromQuery] int? homestayId = null,
			[FromQuery] DateTime? startDate = null,
			[FromQuery] DateTime? endDate = null)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<BookingStatisticsDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
			var isAdmin = roles.Contains("Admin");

			// Host chỉ xem được thống kê của mình
			int? hostId = isAdmin ? null : userId;

			var statistics = await _bookingService.GetBookingStatisticsAsync(
				homestayId, hostId, startDate, endDate);

			return Ok(new ApiResponse<BookingStatisticsDto>
			{
				Success = true,
				Message = "Statistics retrieved successfully",
				Data = statistics
			});
		}

		/// <summary>
		/// Xử lý expired pending bookings (Background job - Admin only)
		/// </summary>
		[HttpPost("process-expired")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ProcessExpiredBookings()
		{
			await _bookingService.ProcessExpiredPendingBookingsAsync();

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Expired bookings processed successfully"
			});
		}
	}
}