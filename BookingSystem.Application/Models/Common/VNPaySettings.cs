namespace BookingSystem.Application.Models.Common
{
	public class VNPaySettings
	{
		public string Url { get; set; } = string.Empty;
		public string ApiUrl { get; set; } = string.Empty;
		public string TmnCode { get; set; } = string.Empty;
		public string HashSecret { get; set; } = string.Empty;
		public string ReturnUrl { get; set; } = string.Empty;
		public string Version { get; set; } = "2.1.0";
		public string Command { get; set; } = "pay";
		public string CurrencyCode { get; set; } = "VND";
		public string Locale { get; set; } = "vn";
	}
}
