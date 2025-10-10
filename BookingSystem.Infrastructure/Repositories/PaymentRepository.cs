using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
	public class PaymentRepository : Repository<Payment>, IPaymentRepository
	{
		public PaymentRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<Payment?> GetByIdWithDetailsAsync(int id)
		{
			return await _dbSet
				.Include(p => p.Booking)
					.ThenInclude(b => b.Guest)
				.Include(p => p.Booking.Homestay)
				.FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId)
		{
			return await _dbSet
				.Where(p => p.BookingId == bookingId)
				.OrderByDescending(p => p.CreatedAt)
				.ToListAsync();
		}

		public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
		{
			return await _dbSet
				.Include(p => p.Booking)
				.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
		}
	}
}