using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Infrastructure.Repositories
{
	public class BookingRepository : Repository<Booking>, IBookingRepository
	{
		public BookingRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<int> GetUserCompletedBookingsCountAsync(int userId)
		{
			return await _dbSet
				.Where(b => b.GuestId == userId && b.BookingStatus == BookingStatus.Completed)
				.CountAsync();
		}

		public async Task<bool> HasUserCompletedBookingAsync(int userId)
		{
			return await _dbSet
				.AnyAsync(b => b.GuestId == userId && b.BookingStatus == BookingStatus.Completed);
		}

		public async Task<Booking?> GetByIdWithDetailsAsync(int id)
		{
			return await _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Homestay)
					.ThenInclude(h => h.Owner)
				.Include(b => b.Homestay.PropertyType)
				.Include(b => b.Homestay.HomestayImages)
				.Include(b => b.Payments)
				.Include(b => b.Reviews)
				.FirstOrDefaultAsync(b => b.Id == id);
		}

		public async Task<Booking?> GetByBookingCodeAsync(string bookingCode)
		{
			return await _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Homestay)
					.ThenInclude(h => h.Owner)
				.Include(b => b.Homestay.PropertyType)
				.Include(b => b.Homestay.HomestayImages)
				.Include(b => b.Payments)
				.Include(b => b.Reviews)
				.FirstOrDefaultAsync(b => b.BookingCode == bookingCode);
		}

		public async Task<PagedResult<Booking>> GetAllBookingsAsync(BookingFilter filter)
		{
			var query = _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Homestay)
					.ThenInclude(h => h.Owner)
				.Include(b => b.Homestay.PropertyType)
				.Include(b => b.Payments)
				.AsQueryable();

			query = ApplyFilters(query, filter);

			var totalCount = await query.CountAsync();

			query = ApplySorting(query, filter);

			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Booking>(items, totalCount, filter.PageNumber, filter.PageSize);
		}

		public async Task<PagedResult<Booking>> GetUserBookingsAsync(int userId, BookingFilter filter)
		{
			var query = _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Homestay)
					.ThenInclude(h => h.Owner)
				.Include(b => b.Homestay.PropertyType)
				.Include(b => b.Homestay.HomestayImages)
				.Include(b => b.Payments)
				.Include(b => b.Reviews)
				.Where(b => b.GuestId == userId);

			query = ApplyFilters(query, filter);

			var totalCount = await query.CountAsync();

			query = ApplySorting(query, filter);

			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Booking>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize
			};
		}

		public async Task<PagedResult<Booking>> GetHostBookingsAsync(int hostId, BookingFilter filter)
		{
			var query = _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Homestay)
					.ThenInclude(h => h.Owner)
				.Include(b => b.Homestay.PropertyType)
				.Include(b => b.Payments)
				.Where(b => b.Homestay.OwnerId == hostId);

			query = ApplyFilters(query, filter);

			var totalCount = await query.CountAsync();

			query = ApplySorting(query, filter);

			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Booking>(items, totalCount, filter.PageNumber, filter.PageSize);
		}

		public async Task<PagedResult<Booking>> GetHomestayBookingsAsync(int homestayId, BookingFilter filter)
		{
			var query = _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Homestay)
				.Include(b => b.Payments)
				.Where(b => b.HomestayId == homestayId);

			query = ApplyFilters(query, filter);

			var totalCount = await query.CountAsync();

			query = ApplySorting(query, filter);

			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<Booking>(items, totalCount, filter.PageNumber, filter.PageSize);
		}

		public async Task<IEnumerable<Booking>> GetOverlappingBookingsAsync(
			int homestayId,
			DateTime checkIn,
			DateTime checkOut,
			int? excludeBookingId = null)
		{
			var query = _dbSet
				.Where(b => b.HomestayId == homestayId
					&& b.BookingStatus != BookingStatus.Cancelled
					&& b.BookingStatus != BookingStatus.Rejected
					&& b.BookingStatus != BookingStatus.NoShow
					&& (
						(b.CheckInDate < checkOut && b.CheckOutDate > checkIn) // Overlapping
					));

			if (excludeBookingId.HasValue)
			{
				query = query.Where(b => b.Id != excludeBookingId.Value);
			}

			return await query.ToListAsync();
		}

		public async Task<bool> HasOverlappingBookingsAsync(
			int homestayId,
			DateTime checkIn,
			DateTime checkOut,
			int? excludeBookingId = null)
		{
			var overlappingBookings = await GetOverlappingBookingsAsync(homestayId, checkIn, checkOut, excludeBookingId);
			return overlappingBookings.Any();
		}

		public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int homestayId)
		{
			var now = DateTime.UtcNow;
			return await _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Payments)
				.Where(b => b.HomestayId == homestayId
					&& b.CheckInDate > now
					&& (b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Pending))
				.OrderBy(b => b.CheckInDate)
				.ToListAsync();
		}

		public async Task<IEnumerable<Booking>> GetActiveBookingsAsync(int homestayId)
		{
			var now = DateTime.UtcNow;
			return await _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Payments)
				.Where(b => b.HomestayId == homestayId
					&& b.CheckInDate <= now
					&& b.CheckOutDate >= now
					&& b.BookingStatus == BookingStatus.CheckedIn)
				.ToListAsync();
		}

		public async Task<decimal> GetTotalRevenueAsync(int hostId, DateTime? startDate = null, DateTime? endDate = null)
		{
			var query = _dbSet
				.Where(b => b.Homestay.OwnerId == hostId
					&& (b.BookingStatus == BookingStatus.Completed || b.BookingStatus == BookingStatus.CheckedOut)
					&& b.Payments.Any(p => p.PaymentStatus == PaymentStatus.Completed));

			if (startDate.HasValue)
			{
				query = query.Where(b => b.CheckOutDate >= startDate.Value);
			}

			if (endDate.HasValue)
			{
				query = query.Where(b => b.CheckInDate <= endDate.Value);
			}

			return await query.SumAsync(b => b.TotalAmount);
		}

		public async Task<int> GetTotalBookingsCountAsync(int homestayId)
		{
			return await _dbSet.CountAsync(b => b.HomestayId == homestayId
				&& (b.BookingStatus == BookingStatus.Completed || b.BookingStatus == BookingStatus.CheckedOut));
		}

		public async Task<bool> HasUserBookedHomestayAsync(int userId, int homestayId)
		{
			return await _dbSet.AnyAsync(b => b.GuestId == userId
				&& b.HomestayId == homestayId
				&& (b.BookingStatus == BookingStatus.Completed || b.BookingStatus == BookingStatus.CheckedOut));
		}

		public async Task<bool> CanUserReviewBookingAsync(int userId, int bookingId)
		{
			var booking = await _dbSet
				.Include(b => b.Reviews)
				.FirstOrDefaultAsync(b => b.Id == bookingId);

			if (booking == null || booking.GuestId != userId)
				return false;

			// Can only review after checkout and if not already reviewed
			return (booking.BookingStatus == BookingStatus.CheckedOut || booking.BookingStatus == BookingStatus.Completed)
				&& !booking.Reviews.Any(r => r.ReviewerId == userId);
		}

		public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status, int? homestayId = null)
		{
			var query = _dbSet
				.Include(b => b.Guest)
				.Include(b => b.Homestay)
				.Include(b => b.Payments)
				.Where(b => b.BookingStatus == status);

			if (homestayId.HasValue)
			{
				query = query.Where(b => b.HomestayId == homestayId.Value);
			}

			return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
		}

		public async Task<IEnumerable<Booking>> GetExpiredPendingBookingsAsync(int expirationMinutes = 30)
		{
			var expirationTime = DateTime.UtcNow.AddMinutes(-expirationMinutes);
			return await _dbSet
				.Where(b => b.BookingStatus == BookingStatus.Pending
					&& b.CreatedAt < expirationTime
					&& !b.Payments.Any(p => p.PaymentStatus == PaymentStatus.Completed))
				.ToListAsync();
		}

		private IQueryable<Booking> ApplyFilters(IQueryable<Booking> query, BookingFilter filter)
		{
			if (!string.IsNullOrWhiteSpace(filter.BookingCode))
			{
				query = query.Where(b => b.BookingCode.Contains(filter.BookingCode));
			}

			if (filter.HomestayId.HasValue)
			{
				query = query.Where(b => b.HomestayId == filter.HomestayId.Value);
			}

			if (filter.GuestId.HasValue)
			{
				query = query.Where(b => b.GuestId == filter.GuestId.Value);
			}

			if (filter.HostId.HasValue)
			{
				query = query.Where(b => b.Homestay.OwnerId == filter.HostId.Value);
			}

			if (filter.Status.HasValue)
			{
				query = query.Where(b => b.BookingStatus == filter.Status.Value);
			}

			if (filter.CheckInDateFrom.HasValue)
			{
				query = query.Where(b => b.CheckInDate >= filter.CheckInDateFrom.Value);
			}

			if (filter.CheckInDateTo.HasValue)
			{
				query = query.Where(b => b.CheckInDate <= filter.CheckInDateTo.Value);
			}

			if (filter.CheckOutDateFrom.HasValue)
			{
				query = query.Where(b => b.CheckOutDate >= filter.CheckOutDateFrom.Value);
			}

			if (filter.CheckOutDateTo.HasValue)
			{
				query = query.Where(b => b.CheckOutDate <= filter.CheckOutDateTo.Value);
			}

			if (filter.MinAmount.HasValue)
			{
				query = query.Where(b => b.TotalAmount >= filter.MinAmount.Value);
			}

			if (filter.MaxAmount.HasValue)
			{
				query = query.Where(b => b.TotalAmount <= filter.MaxAmount.Value);
			}

			return query;
		}

		public async Task<IEnumerable<Booking>> GetUnpaidExpiredBookingsAsync()
		{
			var now = DateTime.UtcNow;
			return await _dbSet
				.Include(b => b.Payments)
				.Include(b => b.Guest)
				.Include(b => b.Homestay)
				.Where(b => b.BookingStatus == BookingStatus.Pending
					&& b.PaymentExpiresAt <= now
					&& !b.Payments.Any(p => p.PaymentStatus == PaymentStatus.Completed))
				.ToListAsync();
		}

		private IQueryable<Booking> ApplySorting(IQueryable<Booking> query, BookingFilter filter)
		{
			return filter.SortBy?.ToLower() switch
			{
				"checkindate" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(b => b.CheckInDate)
					: query.OrderBy(b => b.CheckInDate),
				"checkoutdate" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(b => b.CheckOutDate)
					: query.OrderBy(b => b.CheckOutDate),
				"totalamount" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(b => b.TotalAmount)
					: query.OrderBy(b => b.TotalAmount),
				"status" => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(b => b.BookingStatus)
					: query.OrderBy(b => b.BookingStatus),
				_ => filter.SortDirection.ToLower() == "desc"
					? query.OrderByDescending(b => b.CreatedAt)
					: query.OrderBy(b => b.CreatedAt)
			};
		}
	}
}
