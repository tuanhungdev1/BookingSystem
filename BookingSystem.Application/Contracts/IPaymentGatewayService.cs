using BookingSystem.Application.DTOs.PaymentGatewayDTO;

namespace BookingSystem.Application.Contracts
{
	public interface IPaymentGatewayService
	{
		Task<PaymentGatewayResponse> CreatePaymentUrlAsync(PaymentGatewayRequest request);
		Task<PaymentGatewayCallback> ProcessCallbackAsync(Dictionary<string, string> queryParams);
		Task<PaymentGatewayResponse> RefundAsync(string transactionId, decimal amount);
		Task<PaymentGatewayResponse> QueryTransactionAsync(string transactionId);
	}
}
