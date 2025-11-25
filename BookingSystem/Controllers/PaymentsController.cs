using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.ExportDTOs;
using BookingSystem.Application.DTOs.PaymentDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class PaymentsController : ControllerBase
	{
		private readonly IPaymentService _paymentService;
		private readonly ILogger<PaymentsController> _logger;
		private readonly IGenericExportService _exportService;

		public PaymentsController(
			IPaymentService paymentService,
			IGenericExportService exportService,
			ILogger<PaymentsController> logger)
		{
			_paymentService = paymentService;
			_logger = logger;
			_exportService = exportService;
		}

		private int GetCurrentUserId()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdClaim))
				throw new UnauthorizedAccessException("User ID not found in token.");

			return int.Parse(userIdClaim);
		}

		[HttpGet("export/excel")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<IActionResult> ExportPaymentsToExcel([FromQuery] PaymentFilter filter)
		{
			try
			{
				var hostId = GetCurrentUserId();
				var payments = await _paymentService.GetPaymentsByHostIdAsync(hostId, filter);

				var exportData = payments.Items.Select(p => new PaymentExportDto
				{
					Id = p.Id,
					BookingId = p.BookingId,
					BookingCode = p.Booking?.BookingCode ?? "N/A",
					PaymentAmount = p.PaymentAmount,
					PaymentMethod = p.PaymentMethod.ToString(),
					PaymentStatus = p.PaymentStatus.ToString(),
					TransactionId = p.TransactionId,
					PaymentGateway = p.PaymentGateway,
					ProcessedAt = p.ProcessedAt,
					PaymentNotes = p.PaymentNotes,
					RefundAmount = p.RefundAmount,
					RefundedAt = p.RefundedAt,
					CreatedAt = p.CreatedAt
				});

				var excelData = await _exportService.ExportToExcelAsync(
					exportData,
					sheetName: "Danh sách thanh toán",
					fileName: "Payments_Report.xlsx"
				);

				return File(
					excelData,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					$"Payments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
				);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Export Excel failed for Host {HostId}", GetCurrentUserId());
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Xuất file Excel thất bại. Vui lòng thử lại sau."
				});
			}
		}

		[HttpGet("export/pdf")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<IActionResult> ExportPaymentsToPdf([FromQuery] PaymentFilter filter)
		{
			try
			{
				var hostId = GetCurrentUserId();
				var payments = await _paymentService.GetPaymentsByHostIdAsync(hostId, filter);

				var exportData = payments.Items.Select(p => new PaymentExportDto
				{
					Id = p.Id,
					BookingId = p.BookingId,
					BookingCode = p.Booking?.BookingCode ?? "N/A",
					PaymentAmount = p.PaymentAmount,
					PaymentMethod = p.PaymentMethod.ToString(),
					PaymentStatus = p.PaymentStatus.ToString(),
					TransactionId = p.TransactionId,
					PaymentGateway = p.PaymentGateway,
					ProcessedAt = p.ProcessedAt,
					PaymentNotes = p.PaymentNotes,
					RefundAmount = p.RefundAmount,
					RefundedAt = p.RefundedAt,
					CreatedAt = p.CreatedAt
				});

				var pdfData = await _exportService.ExportToPdfAsync(
					exportData,
					title: "BÁO CÁO THANH TOÁN",
					fileName: "Payments_Report.pdf"
				);

				return File(pdfData, "application/pdf", $"Payments_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Export PDF failed for Host {HostId}", GetCurrentUserId());
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Xuất file PDF thất bại. Vui lòng thử lại sau."
				});
			}
		}

		[HttpGet("export/csv")]
		[Authorize(Roles = "Admin,Host")]
		public async Task<IActionResult> ExportPaymentsToCSV([FromQuery] PaymentFilter filter)
		{
			try
			{
				var hostId = GetCurrentUserId();
				var payments = await _paymentService.GetPaymentsByHostIdAsync(hostId, filter);

				var exportData = payments.Items.Select(p => new PaymentExportDto
				{
					Id = p.Id,
					BookingId = p.BookingId,
					BookingCode = p.Booking?.BookingCode ?? "N/A",
					PaymentAmount = p.PaymentAmount,
					PaymentMethod = p.PaymentMethod.ToString(),
					PaymentStatus = p.PaymentStatus.ToString(),
					TransactionId = p.TransactionId,
					PaymentGateway = p.PaymentGateway,
					ProcessedAt = p.ProcessedAt,
					PaymentNotes = p.PaymentNotes,
					RefundAmount = p.RefundAmount,
					RefundedAt = p.RefundedAt,
					CreatedAt = p.CreatedAt
				});

				var csvData = await _exportService.ExportToCSVAsync(exportData);

				return File(csvData, "text/csv", $"Payments_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Export CSV failed for Host {HostId}", GetCurrentUserId());
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Xuất file CSV thất bại. Vui lòng thử lại sau."
				});
			}
		}

		[HttpGet("host")]
		[Authorize(Roles = "Host,Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetPaymentsByHost(
		[FromQuery] PaymentFilter paymentFilter)
		{
			var hostId = GetCurrentUserId();
			var payments = await _paymentService.GetPaymentsByHostIdAsync(hostId, paymentFilter);

			return Ok(new ApiResponse<PagedResult<PaymentDto>>
			{
				Success = true,
				Message = "Host payments retrieved successfully",
				Data = payments
			});
		}

		[HttpGet("my-payments")]
		public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetMyPayments([FromQuery] PaymentFilter paymentFilter)
		{
			var userId = GetCurrentUserId();
			var payments = await _paymentService.GetAllPaymentAsync(paymentFilter, userId);

			return Ok(new ApiResponse<PagedResult<PaymentDto>>
			{
				Success = true,
				Message = "Payments retrieved successfully",
				Data = payments
			});
		}

		[HttpGet]
		public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetAllPayment([FromQuery] PaymentFilter paymentFilter)
		{
			var payments = await _paymentService.GetAllPaymentAsync(paymentFilter);

			return Ok(new ApiResponse<PagedResult<PaymentDto>>
			{
				Success = true,
				Message = "Payments retrieved successfully",
				Data = payments
			});
		}

		/// <summary>
		/// Create online payment (VNPay, ZaloPay, Momo)
		/// </summary>
		/// <param name="dto">Payment creation details</param>
		/// <returns>Payment URL for redirection</returns>
		[HttpPost("online")]
		public async Task<ActionResult<ApiResponse<PaymentUrlResponseDto>>> CreateOnlinePayment(
			[FromBody] CreateOnlinePaymentDto dto)
		{
			var userId = GetCurrentUserId();
			var result = await _paymentService.CreateOnlinePaymentAsync(userId, dto);

			return Ok(new ApiResponse<PaymentUrlResponseDto>
			{
				Success = true,
				Message = "Payment URL created successfully. Please complete payment on the gateway.",
				Data = result
			});
		}

		/// <summary>
		/// VNPay callback endpoint (IPN - Instant Payment Notification)
		/// This endpoint is called by VNPay server after payment completion
		/// </summary>
		[AllowAnonymous]
		[HttpGet("vnpay-callback")]
		public async Task<IActionResult> VNPayCallback()
		{
			try
			{
				_logger.LogInformation("VNPay callback received");

				var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
				var result = await _paymentService.ProcessPaymentCallbackAsync(PaymentMethod.VNPay, queryParams);

				// VNPay expects a response code
				// Return 200 OK to acknowledge receipt
				return Ok(new
				{
					RspCode = result.PaymentStatus == PaymentStatus.Completed ? "00" : "01",
					Message = result.PaymentStatus == PaymentStatus.Completed ? "Confirm Success" : "Confirm Fail"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing VNPay callback");
				return Ok(new { RspCode = "99", Message = "Unknown error" });
			}
		}

		/// <summary>
		/// VNPay return URL endpoint (User redirected here after payment)
		/// This is where users are redirected after completing payment on VNPay
		/// </summary>
		[AllowAnonymous]
		[HttpGet("vnpay-return")]
		public async Task<IActionResult> VNPayReturn()
		{
			try
			{
				_logger.LogInformation("VNPay return received");

				var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
				var result = await _paymentService.ProcessPaymentCallbackAsync(PaymentMethod.VNPay, queryParams);

				var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
				var redirectUrl = $"{Request.Scheme}://{Request.Host}/payment-callback?{queryString}";

				return Redirect(redirectUrl);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing VNPay return");
				var redirectUrl = $"{Request.Scheme}://{Request.Host}/payment-failure?reason={Uri.EscapeDataString("System error occurred")}";
				return Redirect(redirectUrl);
			}
		}

		/// <summary>
		/// Get VNPay return result as JSON (for API clients)
		/// </summary>
		[AllowAnonymous]
		[HttpGet("vnpay-return-json")]
		public async Task<ActionResult<ApiResponse<PaymentDto>>> VNPayReturnJson()
		{
			var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
			var result = await _paymentService.ProcessPaymentCallbackAsync(PaymentMethod.VNPay, queryParams);

			var success = result.PaymentStatus == PaymentStatus.Completed;

			return Ok(new ApiResponse<PaymentDto>
			{
				Success = success,
				Message = success ? "Payment completed successfully." : "Payment failed.",
				Data = result
			});
		}

		/// <summary>
		/// Create manual payment record (Cash, Bank Transfer)
		/// </summary>
		/// <param name="dto">Payment details</param>
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePayment(
			[FromBody] CreatePaymentDto dto)
		{
			var userId = GetCurrentUserId();
			var result = await _paymentService.CreatePaymentAsync(userId, dto);

			return CreatedAtAction(
				nameof(GetPaymentById),
				new { id = result.Id },
				new ApiResponse<PaymentDto>
				{
					Success = true,
					Message = "Payment record created successfully. Waiting for confirmation.",
					Data = result
				});
		}

		/// <summary>
		/// Process/confirm manual payment (Host/Admin only)
		/// </summary>
		/// <param name="dto">Transaction details</param>
		[Authorize(Roles = "Host,Admin")]
		[HttpPost("process")]
		public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessPayment(
			[FromBody] ProcessPaymentDto dto)
		{
			var result = await _paymentService.ProcessPaymentAsync(dto);

			return Ok(new ApiResponse<PaymentDto>
			{
				Success = true,
				Message = "Payment processed successfully.",
				Data = result
			});
		}

		/// <summary>
		/// Refund payment (Host/Admin only)
		/// </summary>
		/// <param name="id">Payment ID</param>
		/// <param name="dto">Refund details</param>
		[Authorize(Roles = "Host,Admin")]
		[HttpPost("{id:int}/refund")]
		public async Task<ActionResult<ApiResponse<PaymentDto>>> RefundPayment(
			int id,
			[FromBody] RefundPaymentDto dto)
		{

			if (dto.RefundAmount <= 0)
			{
				return BadRequest(new ApiResponse<PaymentDto>
				{
					Success = false,
					Message = "Refund amount must be greater than 0"
				});
			}

			if (string.IsNullOrWhiteSpace(dto.RefundReason))
			{
				return BadRequest(new ApiResponse<PaymentDto>
				{
					Success = false,
					Message = "Refund reason is required"
				});
			}
			var userId = GetCurrentUserId();
			var result = await _paymentService.RefundPaymentAsync(id, userId, dto);

			return Ok(new ApiResponse<PaymentDto>
			{
				Success = true,
				Message = "Payment refunded successfully.",
				Data = result
			});
		}

		/// <summary>
		/// Get payment by ID
		/// </summary>
		/// <param name="id">Payment ID</param>
		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentById(int id)
		{
			var result = await _paymentService.GetByIdAsync(id);

			return Ok(new ApiResponse<PaymentDto>
			{
				Success = true,
				Data = result
			});
		}

		/// <summary>
		/// Get all payments for a specific booking
		/// </summary>
		/// <param name="bookingId">Booking ID</param>
		[HttpGet("booking/{bookingId:int}")]
		public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetPaymentsByBookingId(int bookingId)
		{
			var result = await _paymentService.GetByBookingIdAsync(bookingId);

			return Ok(new ApiResponse<IEnumerable<PaymentDto>>
			{
				Success = true,
				Data = result
			});
		}

		[Authorize(Roles = "Host,Admin")]
		[HttpGet("{id:int}/refund-status")]
		public async Task<ActionResult<ApiResponse<RefundStatusDto>>> GetRefundStatus(int id)
		{
			var result = await _paymentService.GetRefundStatusAsync(id);

			return Ok(new ApiResponse<RefundStatusDto>
			{
				Success = true,
				Message = "Refund status retrieved successfully",
				Data = result
			});
		}

		/// <summary>
		/// Mark payment as failed (Admin only)
		/// </summary>
		/// <param name="id">Payment ID</param>
		/// <param name="dto">Failure reason</param>
		[Authorize(Roles = "Admin")]
		[HttpPost("{id:int}/mark-failed")]
		public async Task<ActionResult<ApiResponse<object>>> MarkAsFailed(
			int id,
			[FromBody] MarkPaymentFailedDto dto)
		{
			var success = await _paymentService.MarkPaymentAsFailedAsync(id, dto.FailureReason);

			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "Payment marked as failed." : "Failed to mark payment as failed."
			});
		}

		[AllowAnonymous]
		[HttpPost("momo-callback")]
		public async Task<IActionResult> MomoCallback()
		{
			try
			{
				_logger.LogInformation("Momo callback received");

				var queryParams = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
				var result = await _paymentService.ProcessPaymentCallbackAsync(PaymentMethod.Momo, queryParams);
				bool success = result.PaymentStatus == PaymentStatus.Completed;
				// Momo expects a JSON response
				return Ok(new
				{
					resultCode = success ? 0 : 1,
					message = success ? "Confirm Success" : "Confirm Fail"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing Momo callback");
				return Ok(new { resultCode = 99, message = "Unknown error" });
			}
		}

		/// <summary>
		/// Momo return URL endpoint (User redirected here after payment)
		/// This is where users are redirected after completing payment on Momo
		/// </summary>
		[AllowAnonymous]
		[HttpGet("momo-return")]
		public async Task<IActionResult> MomoReturn()
		{
			try
			{
				_logger.LogInformation("Momo return received");

				var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
				var result = await _paymentService.ProcessPaymentCallbackAsync(PaymentMethod.Momo, queryParams);

				// Redirect về frontend Momo callback page với query parameters
				var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
				var redirectUrl = $"{Request.Scheme}://{Request.Host}/momo-callback?{queryString}";

				return Redirect(redirectUrl);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing Momo return");
				var redirectUrl = $"{Request.Scheme}://{Request.Host}/momo-callback?resultCode=1006&message={Uri.EscapeDataString("System error occurred")}";
				return Redirect(redirectUrl);
			}
		}

		/// MoMo Return URL (người dùng được redirect về) – nên trả JSON hoặc redirect như cũ đều được
		/// Nhưng nếu muốn đồng bộ và an toàn hơn, nên trả JSON giống VNPay
		[AllowAnonymous]
		[HttpGet("momo-return-json")]
		public async Task<ActionResult<ApiResponse<PaymentDto>>> MomoReturnJson()
		{
			var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
			var result = await _paymentService.ProcessPaymentCallbackAsync(PaymentMethod.Momo, queryParams);

			bool success = result.PaymentStatus == PaymentStatus.Completed;

			return Ok(new ApiResponse<PaymentDto>
			{
				Success = success,
				Message = success ? "Thanh toán MoMo thành công!" : "Thanh toán thất bại hoặc bị hủy.",
				Data = result
			});
		}
	}

	public class MarkPaymentFailedDto
	{
		public string FailureReason { get; set; } = string.Empty;
	}
}