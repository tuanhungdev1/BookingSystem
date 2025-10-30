using BookingSystem.Application.DTOs.BookingDTO;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;

namespace BookingSystem.Application.Contracts
{
	public interface IBookingService
	{
		Task<BookingDto> CreateBookingAsync(int guestId, CreateBookingDto request);
		Task<BookingPriceBreakdownDto> CalculateBookingPriceAsync(BookingPriceCalculationDto request);
		Task<BookingDto?> UpdateBookingAsync(int bookingId, int userId, UpdateBookingDto request);
		Task<BookingDto?> GetByIdAsync(int bookingId, int userId);
		Task<BookingDto?> GetByBookingCodeAsync(string bookingCode);
		Task<PagedResult<BookingDto>> GetUserBookingsAsync(int userId, BookingFilter filter);
		Task<PagedResult<BookingDto>> GetHostBookingsAsync(int hostId, BookingFilter filter);
		Task<PagedResult<BookingDto>> GetAllBookingsAsync(BookingFilter filter);
		Task<bool> ConfirmBookingAsync(int bookingId, int hostId);
		Task<bool> RejectBookingAsync(int bookingId, int hostId, string reason);
		Task<bool> CancelBookingAsync(int bookingId, int userId, CancelBookingDto request);
		Task<bool> CheckInAsync(int bookingId, int hostId);
		Task<bool> CheckOutAsync(int bookingId, int hostId);
		Task<bool> MarkAsCompletedAsync(int bookingId, int userId);
		Task<bool> MarkAsNoShowAsync(int bookingId, int hostId);
		Task<bool> IsHomestayAvailableAsync(int homestayId, DateTime checkInDate, DateTime checkOutDate, int? excludeBookingId = null);
		Task<BookingStatisticsDto> GetBookingStatisticsAsync(int? homestayId = null, int? hostId = null, DateTime? startDate = null, DateTime? endDate = null);
		Task ProcessExpiredPendingBookingsAsync();
	}
}
