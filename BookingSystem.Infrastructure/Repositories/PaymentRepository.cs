using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookingSystem.Infrastructure.Repositories
{
	public class PaymentRepository : Repository<Payment>, IPaymentRepository
	{
		public PaymentRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<PagedResult<Payment>> GetPaymentsByHostIdAsync(int hostId, PaymentFilter filter)
		{
			var query = _dbSet
				.Include(p => p.Booking)
					.ThenInclude(b => b.Guest)
				.Include(p => p.Booking.Homestay)
				.Where(p => p.Booking.Homestay.OwnerId == hostId) // Lọc theo chủ homestay
				.AsQueryable();

			// Áp dụng các filter giống như GetAllPaymentAsync
			if (!string.IsNullOrWhiteSpace(filter.Search))
			{
				var search = filter.Search.Trim().ToLower();
				query = query.Where(p =>
					p.Booking.BookingCode.ToLower().Contains(search) ||
					(p.TransactionId != null && p.TransactionId.ToLower().Contains(search)) ||
					p.Booking.Guest.FullName.ToLower().Contains(search)
				);
			}

			if (!string.IsNullOrWhiteSpace(filter.BookingCode))
			{
				query = query.Where(p => p.Booking.BookingCode.Contains(filter.BookingCode));
			}

			if (filter.PaymentMethod.HasValue)
			{
				query = query.Where(p => p.PaymentMethod == filter.PaymentMethod.Value);
			}

			if (filter.PaymentStatus.HasValue)
			{
				query = query.Where(p => p.PaymentStatus == filter.PaymentStatus.Value);
			}

			if (filter.MinAmount.HasValue)
			{
				query = query.Where(p => p.PaymentAmount >= filter.MinAmount.Value);
			}

			if (filter.MaxAmount.HasValue)
			{
				query = query.Where(p => p.PaymentAmount <= filter.MaxAmount.Value);
			}

			if (filter.DateFrom.HasValue)
			{
				query = query.Where(p => p.CreatedAt >= filter.DateFrom.Value);
			}

			if (filter.DateTo.HasValue)
			{
				query = query.Where(p => p.CreatedAt <= filter.DateTo.Value);
			}

			var totalCount = await query.CountAsync();

			// Sắp xếp
			query = ApplySorting(query, filter);

			// Phân trang
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Payment>(items, totalCount, filter.PageNumber, filter.PageSize);
		}

		public async Task<PagedResult<Payment>> GetAllPaymentAsync(PaymentFilter paymentFilter, int? userId = null)
		{
			var query = _dbSet
				.Include(p => p.Booking)
					.ThenInclude(b => b.Guest)
				.Include(p => p.Booking.Homestay)
				.AsQueryable();

			// 🔹 Lọc theo user (nếu có)
			if (userId != null)
			{
				query = query.Where(p => p.Booking.GuestId == userId);
			}

			// 🔹 Tìm kiếm theo mã BookingCode hoặc TransactionId hoặc tên khách
			if (!string.IsNullOrWhiteSpace(paymentFilter.Search))
			{
				var search = paymentFilter.Search.Trim().ToLower();
				query = query.Where(p =>
					p.Booking.BookingCode.ToLower().Contains(search) ||
					(p.TransactionId != null && p.TransactionId.ToLower().Contains(search)) ||
					p.Booking.Guest.FullName.ToLower().Contains(search)
				);
			}

			// 🔹 Lọc theo mã booking cụ thể
			if (!string.IsNullOrWhiteSpace(paymentFilter.BookingCode))
			{
				query = query.Where(p => p.Booking.BookingCode.Contains(paymentFilter.BookingCode));
			}

			// 🔹 Lọc theo phương thức thanh toán
			if (paymentFilter.PaymentMethod.HasValue)
			{
				query = query.Where(p => p.PaymentMethod == paymentFilter.PaymentMethod.Value);
			}

			// 🔹 Lọc theo trạng thái thanh toán
			if (paymentFilter.PaymentStatus.HasValue)
			{
				query = query.Where(p => p.PaymentStatus == paymentFilter.PaymentStatus.Value);
			}

			// 🔹 Lọc theo khoảng tiền
			if (paymentFilter.MinAmount.HasValue)
			{
				query = query.Where(p => p.PaymentAmount >= paymentFilter.MinAmount.Value);
			}
			if (paymentFilter.MaxAmount.HasValue)
			{
				query = query.Where(p => p.PaymentAmount <= paymentFilter.MaxAmount.Value);
			}

			// 🔹 Lọc theo thời gian xử lý
			if (paymentFilter.DateFrom.HasValue)
			{
				query = query.Where(p => p.CreatedAt >= paymentFilter.DateFrom.Value);
			}
			if (paymentFilter.DateTo.HasValue)
			{
				query = query.Where(p => p.CreatedAt <= paymentFilter.DateTo.Value);
			}
			var totalCount = await query.CountAsync();

			// 🔹 Sắp xếp
			query = ApplySorting(query, paymentFilter);

			// 🔹 Phân trang
			var items = await query
				.Skip((paymentFilter.PageNumber - 1) * paymentFilter.PageSize)
				.Take(paymentFilter.PageSize)
				.ToListAsync();

			return new PagedResult<Payment>(items, totalCount, paymentFilter.PageNumber, paymentFilter.PageSize);
		}

		private IQueryable<Payment> ApplySorting(IQueryable<Payment> query, PaymentFilter filter)
		{
			var sortBy = filter.SortBy?.ToLower() ?? "createdat";
			var sortDirection = filter.SortDirection?.ToLower() ?? "desc";

			return sortBy switch
			{
				"paymentamount" => sortDirection == "desc"
					? query.OrderByDescending(p => p.PaymentAmount)
					: query.OrderBy(p => p.PaymentAmount),

				"processedat" => sortDirection == "desc"
					? query.OrderByDescending(p => p.ProcessedAt)
					: query.OrderBy(p => p.ProcessedAt),

				_ => sortDirection == "desc"
					? query.OrderByDescending(p => p.CreatedAt)
					: query.OrderBy(p => p.CreatedAt),
			};
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