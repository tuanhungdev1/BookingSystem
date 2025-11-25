using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.PaymentGatewayDTO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Services
{
	/// <summary>
	/// Dịch vụ xử lý thanh toán tiền mặt (offline payment)
	/// Thanh toán tiền mặt không cần tích hợp cổng thanh toán
	/// Chỉ cần ghi nhận yêu cầu thanh toán, host sẽ xác nhận khi nhận tiền
	/// </summary>
	public class CashPaymentService : IPaymentGatewayService
	{
		private readonly ILogger<CashPaymentService> _logger;

		public CashPaymentService(ILogger<CashPaymentService> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Thanh toán tiền mặt không có URL, chỉ ghi nhận yêu cầu
		/// </summary>
		public async Task<PaymentGatewayResponse> CreatePaymentUrlAsync(PaymentGatewayRequest request)
		{
			try
			{
				_logger.LogInformation("Creating cash payment record for booking {BookingId}", request.BookingId);

				var transactionId = $"CASH_{request.BookingId}_{DateTime.UtcNow.Ticks}";

				return await Task.FromResult(new PaymentGatewayResponse
				{
					Success = true,
					PaymentUrl = null, // Không có URL cho thanh toán tiền mặt
					TransactionId = transactionId,
					Message = "Yêu cầu thanh toán tiền mặt đã được tạo. Vui lòng liên hệ chủ nhà để hoàn tất thanh toán."
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating cash payment for booking {BookingId}", request.BookingId);
				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Error creating cash payment: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// Xử lý callback cho thanh toán tiền mặt
		/// Trong thực tế, thanh toán tiền mặt được xác nhận thủ công bởi host
		/// </summary>
		public async Task<PaymentGatewayCallback> ProcessCallbackAsync(Dictionary<string, string> callbackData)
		{
			try
			{
				_logger.LogInformation("Processing cash payment callback");

				return await Task.FromResult(new PaymentGatewayCallback
				{
					Success = true,
					TransactionId = callbackData.ContainsKey("transactionId")
						? callbackData["transactionId"]
						: $"CASH_{DateTime.UtcNow.Ticks}",
					OrderId = callbackData.ContainsKey("bookingId") ? callbackData["bookingId"] : "0",
					Amount = callbackData.ContainsKey("amount") ? decimal.Parse(callbackData["amount"]) : 0,
					ResponseCode = "00",
					Message = "Thanh toán tiền mặt đã được ghi nhận",
					TransactionDate = DateTime.UtcNow,
					BankCode = "CASH",
					RawData = callbackData
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing cash payment callback");
				return new PaymentGatewayCallback
				{
					Success = false,
					Message = $"Error processing callback: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// Hoàn tiền cho thanh toán tiền mặt
		/// </summary>
		public async Task<PaymentGatewayResponse> RefundAsync(string transactionId, decimal amount)
		{
			try
			{
				_logger.LogInformation("Processing cash refund for transaction {TransactionId}", transactionId);

				return await Task.FromResult(new PaymentGatewayResponse
				{
					Success = true,
					TransactionId = transactionId,
					Message = $"Yêu cầu hoàn tiền {amount:C} đã được ghi nhận. Host sẽ xử lý hoàn tiền tiền mặt."
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing cash refund");
				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Error processing refund: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// Truy vấn trạng thái thanh toán tiền mặt
		/// </summary>
		public async Task<PaymentGatewayResponse> QueryTransactionAsync(string transactionId)
		{
			try
			{
				_logger.LogInformation("Querying cash transaction {TransactionId}", transactionId);

				return await Task.FromResult(new PaymentGatewayResponse
				{
					Success = true,
					TransactionId = transactionId,
					Message = "Ghi nhận yêu cầu thanh toán tiền mặt"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error querying cash transaction");
				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Error querying transaction: {ex.Message}"
				};
			}
		}
	}
}
