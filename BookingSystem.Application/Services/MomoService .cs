using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.PaymentGatewayDTO;
using BookingSystem.Application.Models.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookingSystem.Application.Services
{
	public class MomoService : IPaymentGatewayService
	{
		private readonly MomoSettings _settings;
		private readonly ILogger<MomoService> _logger;
		private readonly HttpClient _httpClient;

		public MomoService(
			IOptions<PaymentGatewaySettings> settings,
			ILogger<MomoService> logger,
			HttpClient httpClient)
		{
			_settings = settings.Value.Momo;
			_logger = logger;
			_httpClient = httpClient;
		}

		public async Task<PaymentGatewayResponse> CreatePaymentUrlAsync(PaymentGatewayRequest request)
		{
			try
			{
				_logger.LogInformation("Creating Momo payment URL for booking {BookingId}", request.BookingId);

				var orderId = $"{request.BookingId}_{DateTime.UtcNow.Ticks}";
				var requestId = Guid.NewGuid().ToString();

				// Tạo raw signature
				var rawSignature = $"accessKey={_settings.AccessKey}" +
					$"&amount={request.Amount}" +
					$"&extraData=" +
					$"&ipnUrl={_settings.IpnUrl}" +
					$"&orderId={orderId}" +
					$"&orderInfo={request.OrderInfo}" +
					$"&partnerCode={_settings.PartnerCode}" +
					$"&redirectUrl={request.ReturnUrl}" +
					$"&requestId={requestId}" +
					$"&requestType=payWithMethod";

				var signature = GenerateSignature(rawSignature, _settings.SecretKey);

				var payload = new
				{
					partnerCode = _settings.PartnerCode,
					accessKey = _settings.AccessKey,
					requestId = requestId,
					amount = (long)request.Amount,
					orderId = orderId,
					orderInfo = request.OrderInfo,
					redirectUrl = request.ReturnUrl,
					ipnUrl = _settings.IpnUrl,
					extraData = string.Empty,
					requestType = "payWithMethod",
					signature = signature,
					lang = "vi",
					autoCapture = true
				};

				var content = new StringContent(
					JsonSerializer.Serialize(payload),
					Encoding.UTF8,
					"application/json");

				var response = await _httpClient.PostAsync(_settings.Url, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				_logger.LogInformation("Momo API response: {Response}", responseContent);

				var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

				if (jsonResponse.TryGetProperty("resultCode", out var resultCode) && resultCode.GetInt32() == 0)
				{
					var payUrl = jsonResponse.GetProperty("payUrl").GetString();

					return new PaymentGatewayResponse
					{
						Success = true,
						PaymentUrl = payUrl,
						TransactionId = orderId,
						Message = "Payment URL created successfully"
					};
				}

				var errorMessage = jsonResponse.TryGetProperty("message", out var msg)
					? msg.GetString()
					: "Unknown error";

				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Error creating payment URL: {errorMessage}"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating Momo payment URL for booking {BookingId}", request.BookingId);
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
				_logger.LogInformation("Processing Momo callback");

				// Xác thực signature
				if (!queryParams.ContainsKey("signature"))
				{
					_logger.LogWarning("Missing signature in Momo callback");
					return new PaymentGatewayCallback
					{
						Success = false,
						Message = "Missing signature"
					};
				}

				var signature = queryParams["signature"];
				var rawSignature = GenerateRawSignatureFromCallback(queryParams);
				var computedSignature = GenerateSignature(rawSignature, _settings.SecretKey);

				if (signature != computedSignature)
				{
					_logger.LogWarning("Invalid Momo signature");
					return new PaymentGatewayCallback
					{
						Success = false,
						Message = "Invalid signature"
					};
				}

				// Extract dữ liệu từ callback
				var orderId = queryParams.ContainsKey("orderId") ? queryParams["orderId"] : string.Empty;
				var resultCode = queryParams.ContainsKey("resultCode") ? int.Parse(queryParams["resultCode"]) : -1;
				var transactionId = queryParams.ContainsKey("transId") ? queryParams["transId"].ToString() : string.Empty;
				var amount = queryParams.ContainsKey("amount") ? long.Parse(queryParams["amount"]) : 0;
				var responseTime = queryParams.ContainsKey("responseTime") ? queryParams["responseTime"] : string.Empty;

				// Extract booking ID từ orderId
				var bookingId = orderId.Split('_')[0];

				var success = resultCode == 0;
				var message = GetResponseMessage(resultCode);

				// Parse transaction date
				DateTime transactionDate = DateTime.UtcNow;
				if (!string.IsNullOrEmpty(responseTime) && long.TryParse(responseTime, out var timestamp))
				{
					transactionDate = UnixTimeStampToDateTime(timestamp);
				}

				_logger.LogInformation(
					"Momo callback processed: OrderId={OrderId}, ResultCode={ResultCode}, Success={Success}",
					orderId, resultCode, success);

				return await Task.FromResult(new PaymentGatewayCallback
				{
					Success = success,
					TransactionId = transactionId,
					OrderId = bookingId,
					Amount = amount / 100m, // Momo trả về amount * 100
					ResponseCode = resultCode.ToString(),
					Message = message,
					TransactionDate = transactionDate,
					BankCode = "Momo",
					RawData = queryParams
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing Momo callback");
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
				_logger.LogInformation("Processing Momo refund for transaction {TransactionId}", transactionId);

				var requestId = Guid.NewGuid().ToString();
				var rawSignature = $"accessKey={_settings.AccessKey}" +
					$"&amount={(long)(amount * 100)}" +
					$"&description=Refund Payment" +
					$"&orderId={transactionId}" +
					$"&partnerCode={_settings.PartnerCode}" +
					$"&requestId={requestId}" +
					$"&transId={transactionId}";

				var signature = GenerateSignature(rawSignature, _settings.SecretKey);

				var payload = new
				{
					partnerCode = _settings.PartnerCode,
					accessKey = _settings.AccessKey,
					requestId = requestId,
					orderId = transactionId,
					amount = (long)(amount * 100),
					transId = transactionId,
					description = "Refund Payment",
					signature = signature,
					lang = "vi"
				};

				var content = new StringContent(
					JsonSerializer.Serialize(payload),
					Encoding.UTF8,
					"application/json");

				var response = await _httpClient.PostAsync(_settings.RefundUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

				if (jsonResponse.TryGetProperty("resultCode", out var resultCode) && resultCode.GetInt32() == 0)
				{
					return new PaymentGatewayResponse
					{
						Success = true,
						TransactionId = transactionId,
						Message = "Refund processed successfully"
					};
				}

				var errorMessage = jsonResponse.TryGetProperty("message", out var msg)
					? msg.GetString()
					: "Unknown error";

				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Refund failed: {errorMessage}"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing Momo refund");
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
				_logger.LogInformation("Querying Momo transaction {TransactionId}", transactionId);

				var requestId = Guid.NewGuid().ToString();
				var rawSignature = $"accessKey={_settings.AccessKey}" +
					$"&orderId={transactionId}" +
					$"&partnerCode={_settings.PartnerCode}" +
					$"&requestId={requestId}";

				var signature = GenerateSignature(rawSignature, _settings.SecretKey);

				var payload = new
				{
					partnerCode = _settings.PartnerCode,
					accessKey = _settings.AccessKey,
					requestId = requestId,
					orderId = transactionId,
					signature = signature,
					lang = "vi"
				};

				var content = new StringContent(
					JsonSerializer.Serialize(payload),
					Encoding.UTF8,
					"application/json");

				var response = await _httpClient.PostAsync(_settings.QueryUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

				if (jsonResponse.TryGetProperty("resultCode", out var resultCode) && resultCode.GetInt32() == 0)
				{
					return new PaymentGatewayResponse
					{
						Success = true,
						TransactionId = transactionId,
						Message = "Transaction found"
					};
				}

				return new PaymentGatewayResponse
				{
					Success = false,
					Message = "Transaction not found or failed"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error querying Momo transaction");
				return new PaymentGatewayResponse
				{
					Success = false,
					Message = $"Error querying transaction: {ex.Message}"
				};
			}
		}

		private string GenerateSignature(string rawSignature, string secretKey)
		{
			using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
			{
				var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawSignature));
				return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
			}
		}

		private string GenerateRawSignatureFromCallback(Dictionary<string, string> queryParams)
		{
			// MoMo yêu cầu các field phải đúng thứ tự này (A-Z) và không có field thừa/thiếu
			var rawData = new[]
			{
		$"accessKey={_settings.AccessKey}",
		$"amount={GetValueOrEmpty("amount")}",
		$"extraData={GetValueOrEmpty("extraData")}",
		$"message={GetValueOrEmpty("message")}",
		$"orderId={GetValueOrEmpty("orderId")}",
		$"orderInfo={GetValueOrEmpty("orderInfo")}",
		$"orderType={GetValueOrEmpty("orderType")}",
		$"partnerCode={_settings.PartnerCode}",
		$"payType={GetValueOrEmpty("payType")}",
		$"requestId={GetValueOrEmpty("requestId")}",
		$"responseTime={GetValueOrEmpty("responseTime")}",
		$"resultCode={GetValueOrEmpty("resultCode")}",
		$"transId={GetValueOrEmpty("transId")}"
	};

			var rawSignature = string.Join("&", rawData);
			_logger.LogDebug("MoMo raw signature string: {RawSignature}", rawSignature);

			return rawSignature;

			// Helper để tránh lỗi tên trùng + code sạch hơn
			string GetValueOrEmpty(string key) =>
				queryParams.TryGetValue(key, out var value) ? value : string.Empty;
		}

		private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
		{
			var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToUniversalTime();
			return dateTime;
		}

		private static string GetResponseMessage(int resultCode)
		{
			return resultCode switch
			{
				0 => "Giao dịch thành công",
				1001 => "Yêu cầu không hợp lệ",
				1002 => "Lỗi khi xác thực merchant",
				1003 => "Yêu cầu bị từ chối",
				1004 => "Số tiền không hợp lệ",
				1005 => "Lỗi không xác định",
				1006 => "Giao dịch không thành công",
				9000 => "Giao dịch đã được xử lý trước đó",
				9001 => "Giao dịch đang được xử lý",
				_ => "Giao dịch thất bại"
			};
		}
	}
}
