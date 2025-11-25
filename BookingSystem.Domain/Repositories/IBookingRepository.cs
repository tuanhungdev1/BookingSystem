using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Repositories
{
	public interface IBookingRepository : IRepository<Booking>
	{
		Task<Booking?> GetByIdWithDetailsAsync(int id);
		Task<Booking?> GetByBookingCodeAsync(string bookingCode);
		Task<PagedResult<Booking>> GetAllBookingsAsync(BookingFilter filter);
		Task<PagedResult<Booking>> GetUserBookingsAsync(int userId, BookingFilter filter);
		Task<PagedResult<Booking>> GetHostBookingsAsync(int hostId, BookingFilter filter);
		Task<PagedResult<Booking>> GetHomestayBookingsAsync(int homestayId, BookingFilter filter);
		Task<IEnumerable<Booking>> GetOverlappingBookingsAsync(int homestayId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);
		Task<bool> HasOverlappingBookingsAsync(int homestayId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);
		Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int homestayId);
		Task<IEnumerable<Booking>> GetActiveBookingsAsync(int homestayId);
		Task<decimal> GetTotalRevenueAsync(int hostId, DateTime? startDate = null, DateTime? endDate = null);
		Task<int> GetTotalBookingsCountAsync(int homestayId);
		Task<bool> HasUserBookedHomestayAsync(int userId, int homestayId);
		Task<bool> CanUserReviewBookingAsync(int userId, int bookingId);
		Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status, int? homestayId = null);
		Task<IEnumerable<Booking>> GetExpiredPendingBookingsAsync(int expirationMinutes = 30);
		Task<IEnumerable<Booking>> GetUnpaidExpiredBookingsAsync();
		Task<int> GetUserCompletedBookingsCountAsync(int userId);
		Task<bool> HasUserCompletedBookingAsync(int userId);
	}
}
