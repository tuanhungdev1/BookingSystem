using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
