namespace BookingSystem.Application.Models.Common
{
	public class PaymentGatewaySettings
	{
		public VNPaySettings VNPay { get; set; } = new VNPaySettings();
		// Future expansion
		// public ZaloPaySettings ZaloPay { get; set; } = new ZaloPaySettings();
		// public MomoSettings Momo { get; set; } = new MomoSettings();
	}
}
