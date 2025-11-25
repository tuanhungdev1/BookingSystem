using BookingSystem.Application.Contracts;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Enums;
using BookingSystem.Infrastructure.PaymentGateways;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Application.Factories
{
	public interface IPaymentGatewayFactory
	{
		IPaymentGatewayService GetPaymentGateway(PaymentMethod paymentMethod);
	}

	public class PaymentGatewayFactory : IPaymentGatewayFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public PaymentGatewayFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IPaymentGatewayService GetPaymentGateway(PaymentMethod paymentMethod)
		{
			return paymentMethod switch
			{
				PaymentMethod.VNPay => _serviceProvider.GetRequiredService<VNPayService>(),
				PaymentMethod.Momo => _serviceProvider.GetRequiredService<MomoService>(),
				PaymentMethod.Cash => _serviceProvider.GetRequiredService<CashPaymentService>(),
				// Future expansion:
				// PaymentMethod.ZaloPay => _serviceProvider.GetRequiredService<ZaloPayService>(),
				// PaymentMethod.Momo => _serviceProvider.GetRequiredService<MomoService>(),
				_ => throw new NotSupportedException($"Payment method {paymentMethod} is not supported")
			};
		}
	}
}
