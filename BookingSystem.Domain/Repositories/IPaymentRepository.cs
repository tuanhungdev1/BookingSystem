using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Repositories
{
	public interface IPaymentRepository : IRepository<Payment>
	{
		Task<PagedResult<Payment>> GetAllPaymentAsync(PaymentFilter paymentFilter, int? userId = null);
		Task<Payment?> GetByIdWithDetailsAsync(int id);
		Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId);
		Task<Payment?> GetByTransactionIdAsync(string transactionId);
		Task<PagedResult<Payment>> GetPaymentsByHostIdAsync(int hostId, PaymentFilter filter);
	}
}
