using BookingSystem.Domain.Base;
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
		Task<Payment?> GetByIdWithDetailsAsync(int id);
		Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId);
		Task<Payment?> GetByTransactionIdAsync(string transactionId);
	}
}
