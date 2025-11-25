namespace BookingSystem.Application.Models.Common
{
	public class PaymentGatewaySettings
	{
		public VNPaySettings VNPay { get; set; } = new VNPaySettings();
		public MomoSettings Momo { get; set; } = new MomoSettings();
	}
}
