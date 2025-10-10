using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BookingSystem.Infrastructure.PaymentGateways.VNPay
{
	public class VnPayLibrary
	{
		private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
		private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

		public void AddRequestData(string key, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				_requestData.Add(key, value);
			}
		}

		public void AddResponseData(string key, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				_responseData.Add(key, value);
			}
		}

		public string GetResponseData(string key)
		{
			return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
		}

		public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
		{
			var data = new StringBuilder();
			foreach (var (key, value) in _requestData)
			{
				if (!string.IsNullOrEmpty(value))
				{
					data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
				}
			}

			var queryString = data.ToString();
			if (queryString.Length > 0)
			{
				queryString = queryString.Remove(queryString.Length - 1, 1);
			}

			var signData = queryString;
			var vnpSecureHash = HmacSHA512(vnpHashSecret, signData);

			return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
		}

		public bool ValidateSignature(string inputHash, string secretKey)
		{
			var rspRaw = GetResponseData();
			var myChecksum = HmacSHA512(secretKey, rspRaw);
			return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
		}

		private string GetResponseData()
		{
			var data = new StringBuilder();

			if (_responseData.ContainsKey("vnp_SecureHashType"))
			{
				_responseData.Remove("vnp_SecureHashType");
			}
			if (_responseData.ContainsKey("vnp_SecureHash"))
			{
				_responseData.Remove("vnp_SecureHash");
			}

			foreach (var (key, value) in _responseData)
			{
				if (!string.IsNullOrEmpty(value))
				{
					data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
				}
			}

			if (data.Length > 0)
			{
				data.Remove(data.Length - 1, 1);
			}

			return data.ToString();
		}

		private static string HmacSHA512(string key, string inputData)
		{
			var hash = new StringBuilder();
			var keyBytes = Encoding.UTF8.GetBytes(key);
			var inputBytes = Encoding.UTF8.GetBytes(inputData);

			using var hmac = new HMACSHA512(keyBytes);
			var hashValue = hmac.ComputeHash(inputBytes);

			foreach (var theByte in hashValue)
			{
				hash.Append(theByte.ToString("x2"));
			}

			return hash.ToString();
		}
	}

	public class VnPayCompare : IComparer<string>
	{
		public int Compare(string? x, string? y)
		{
			if (x == y) return 0;
			if (x == null) return -1;
			if (y == null) return 1;

			var vnpCompare = CompareInfo.GetCompareInfo("en-US");
			return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
		}
	}
}