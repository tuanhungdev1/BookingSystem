using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Models.Common
{
	public class MomoSettings
	{
		public string PartnerCode { get; set; } = string.Empty;
		public string AccessKey { get; set; } = string.Empty;
		public string SecretKey { get; set; } = string.Empty;
		public string Url { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
		public string QueryUrl { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/query";
		public string RefundUrl { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/refund";
		public string RedirectUrl { get; set; } = string.Empty;
		public string IpnUrl { get; set; } = string.Empty;
	}
}
