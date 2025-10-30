using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AmenityDTO;
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

		public PaymentsController(
			IPaymentService paymentService,
			ILogger<PaymentsController> logger)
		{
			_paymentService = paymentService;
			_logger = logger;
		}

		private int GetCurrentUserId()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdClaim))
				throw new UnauthorizedAccessException("User ID not found in token.");

			return int.Parse(userIdClaim);
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
	}

	public class MarkPaymentFailedDto
	{
		public string FailureReason { get; set; } = string.Empty;
	}
}