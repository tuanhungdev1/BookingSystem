using BookingSystem.Application.DTOs.PaymentDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.Contracts
{
	public interface IPaymentService
	{
		Task<PagedResult<PaymentDto>> GetAllPaymentAsync(PaymentFilter paymentFilter, int? userId = null);
		Task<PagedResult<PaymentDto>> GetPaymentsByHostIdAsync(int hostId, PaymentFilter filter);
		// Online payment methods
		Task<PaymentUrlResponseDto> CreateOnlinePaymentAsync(int userId, CreateOnlinePaymentDto request);
		Task<PaymentDto> ProcessPaymentCallbackAsync(PaymentMethod paymentMethod, Dictionary<string, string> callbackData);

		// Manual payment methods
		Task<PaymentDto> CreatePaymentAsync(int userId, CreatePaymentDto request);
		Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto request);

		// Common methods
		Task<PaymentDto> RefundPaymentAsync(int paymentId, int userId, RefundPaymentDto request);
		Task<PaymentDto?> GetByIdAsync(int paymentId);
		Task<IEnumerable<PaymentDto>> GetByBookingIdAsync(int bookingId);
		Task<bool> MarkPaymentAsFailedAsync(int paymentId, string failureReason);

		Task<RefundStatusDto> GetRefundStatusAsync(int paymentId);
	}
}
