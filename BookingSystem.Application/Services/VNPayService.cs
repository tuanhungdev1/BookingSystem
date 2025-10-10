
using BookingSystem.Infrastructure.PaymentGateways.VNPay;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Globalization;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.PaymentGatewayDTO;
using BookingSystem.Application.Models.Common;

namespace BookingSystem.Infrastructure.PaymentGateways
{
	public class VNPayService : IPaymentGatewayService
	{
		private readonly VNPaySettings _settings;
		private readonly ILogger<VNPayService> _logger;

		public VNPayService(
			IOptions<PaymentGatewaySettings> settings,
			ILogger<VNPayService> logger)
		{
			_settings = settings.Value.VNPay;
			_logger = logger;
		}

		public async Task<PaymentGatewayResponse> CreatePaymentUrlAsync(PaymentGatewayRequest request)
		{
			try
			{
				_logger.LogInformation("Creating VNPay payment URL for booking {BookingId}", request.BookingId);

				var vnpay = new VnPayLibrary();
				var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
				var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
				var tick = timeNow.Ticks.ToString();
				var vnpTxnRef = $"{request.BookingId}_{tick}"; // Unique transaction reference

				// Add request data
				vnpay.AddRequestData("vnp_Version", _settings.Version);
				vnpay.AddRequestData("vnp_Command", _settings.Command);
				vnpay.AddRequestData("vnp_TmnCode", _settings.TmnCode);
				vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString()); // VNPay requires amount in smallest unit
				vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
				vnpay.AddRequestData("vnp_CurrCode", _settings.CurrencyCode);
				vnpay.AddRequestData("vnp_IpAddr", request.IpAddress);
				vnpay.AddRequestData("vnp_Locale", _settings.Locale);
				vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
				vnpay.AddRequestData("vnp_OrderType", "other"); // Bill payment
				vnpay.AddRequestData("vnp_ReturnUrl", request.ReturnUrl);
				vnpay.AddRequestData("vnp_TxnRef", vnpTxnRef);

				// Optional: Add expire date (default 15 minutes)
				var expireDate = timeNow.AddMinutes(15);
				vnpay.AddRequestData("vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss"));

				var paymentUrl = vnpay.CreateRequestUrl(_settings.Url, _settings.HashSecret);

				_logger.LogInformation("VNPay payment URL created successfully for booking {BookingId}", request.BookingId);

				return await Task.FromResult(new PaymentGatewayResponse
				{
					Success = true,
					PaymentUrl = paymentUrl,
					TransactionId = vnpTxnRef,
					Message = "Payment URL created successfully"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating VNPay payment URL for booking {BookingId}", request.BookingId);
				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Error creating payment URL: {ex.Message}"
				};
			}
		}

		public async Task<PaymentGatewayCallback> ProcessCallbackAsync(Dictionary<string, string> queryParams)
		{
			try
			{
				_logger.LogInformation("Processing VNPay callback");

				var vnpay = new VnPayLibrary();

				foreach (var (key, value) in queryParams)
				{
					if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
					{
						vnpay.AddResponseData(key, value);
					}
				}

				var vnpSecureHash = queryParams.ContainsKey("vnp_SecureHash")
					? queryParams["vnp_SecureHash"]
					: string.Empty;

				var isValidSignature = vnpay.ValidateSignature(vnpSecureHash, _settings.HashSecret);

				if (!isValidSignature)
				{
					_logger.LogWarning("Invalid VNPay signature");
					return new PaymentGatewayCallback
					{
						Success = false,
						Message = "Invalid signature"
					};
				}

				var vnpTxnRef = vnpay.GetResponseData("vnp_TxnRef");
				var vnpResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
				var vnpTransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
				var vnpAmount = vnpay.GetResponseData("vnp_Amount");
				var vnpBankCode = vnpay.GetResponseData("vnp_BankCode");
				var vnpPayDate = vnpay.GetResponseData("vnp_PayDate");

				// Extract booking ID from transaction reference
				var bookingId = vnpTxnRef.Split('_')[0];

				var success = vnpResponseCode == "00";
				var message = GetResponseMessage(vnpResponseCode);

				// Parse amount (VNPay returns amount * 100)
				var amount = decimal.Parse(vnpAmount) / 100;

				// Parse transaction date
				DateTime transactionDate;
				if (!DateTime.TryParseExact(vnpPayDate, "yyyyMMddHHmmss",
					CultureInfo.InvariantCulture, DateTimeStyles.None, out transactionDate))
				{
					transactionDate = DateTime.UtcNow;
				}

				_logger.LogInformation(
					"VNPay callback processed: TxnRef={TxnRef}, ResponseCode={ResponseCode}, Success={Success}",
					vnpTxnRef, vnpResponseCode, success);

				return await Task.FromResult(new PaymentGatewayCallback
				{
					Success = success,
					TransactionId = vnpTransactionNo,
					OrderId = bookingId,
					Amount = amount,
					ResponseCode = vnpResponseCode,
					Message = message,
					TransactionDate = transactionDate,
					BankCode = vnpBankCode,
					RawData = queryParams
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing VNPay callback");
				return new PaymentGatewayCallback
				{
					Success = false,
					Message = $"Error processing callback: {ex.Message}"
				};
			}
		}

		public async Task<PaymentGatewayResponse> RefundAsync(string transactionId, decimal amount)
		{
			try
			{
				_logger.LogInformation("Processing VNPay refund for transaction {TransactionId}", transactionId);

				// TODO: Implement VNPay refund API call
				// This requires calling VNPay API with proper authentication

				await Task.CompletedTask;

				return new PaymentGatewayResponse
				{
					Success = false,
					Message = "Refund functionality not yet implemented. Please contact VNPay support."
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing VNPay refund");
				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Error processing refund: {ex.Message}"
				};
			}
		}

		public async Task<PaymentGatewayResponse> QueryTransactionAsync(string transactionId)
		{
			try
			{
				_logger.LogInformation("Querying VNPay transaction {TransactionId}", transactionId);

				// TODO: Implement VNPay query transaction API call

				await Task.CompletedTask;

				return new PaymentGatewayResponse
				{
					Success = false,
					Message = "Query transaction functionality not yet implemented."
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error querying VNPay transaction");
				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Error querying transaction: {ex.Message}"
				};
			}
		}

		private static string GetResponseMessage(string responseCode)
		{
			return responseCode switch
			{
				"00" => "Giao dịch thành công",
				"07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
				"09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
				"10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
				"11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
				"12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
				"13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
				"24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
				"51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
				"65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
				"75" => "Ngân hàng thanh toán đang bảo trì.",
				"79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
				_ => "Giao dịch thất bại"
			};
		}
	}
}