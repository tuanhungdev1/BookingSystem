using BookingSystem.Application.DTOs.CouponDTO;
using BookingSystem.Application.DTOs.PaymentDTO;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class BookingDto
	{
		public int Id { get; set; }
		public string BookingCode { get; set; } = string.Empty;
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public int NumberOfNights { get; set; }
		public int NumberOfGuests { get; set; }
		public int NumberOfAdults { get; set; }
		public int NumberOfChildren { get; set; }
		public int NumberOfInfants { get; set; }
		public decimal BaseAmount { get; set; }
		public decimal CleaningFee { get; set; }
		public decimal ServiceFee { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal TotalAmount { get; set; }
		public BookingStatus BookingStatus { get; set; }
		public PaymentStatus PaymentStatus { get; set; }
		public string BookingStatusDisplay { get; set; } = string.Empty;
		public string? SpecialRequests { get; set; }
		public string? CancellationReason { get; set; }
		public DateTime? CancelledAt { get; set; }
		public string? CancelledBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string? PaymentNotes { get; set; }
		public decimal? CouponDiscountAmount { get; set; }
		public List<CouponUsageDto> AppliedCoupons { get; set; } = new List<CouponUsageDto>();
		// Guest Information
		public int GuestId { get; set; }
		public string GuestName { get; set; } = string.Empty;
		public string? GuestPhone { get; set; }
		public string? GuestAvatar { get; set; }

		// Thông tin người đặt
		public string GuestFullName { get; set; } = string.Empty;
		public string GuestEmail { get; set; } = string.Empty;
		public string GuestPhoneNumber { get; set; } = string.Empty;
		public string? GuestAddress { get; set; }
		public string? GuestCity { get; set; }
		public string? GuestCountry { get; set; }

		// Thông tin người ở thực tế
		public bool IsBookingForSomeoneElse { get; set; } = false;
		public string? ActualGuestFullName { get; set; }
		public string? ActualGuestEmail { get; set; }
		public string? ActualGuestPhoneNumber { get; set; }
		public string? ActualGuestIdNumber { get; set; }
		public string? ActualGuestNotes { get; set; }

		// Homestay Information
		public BookingHomestayDto Homestay { get; set; } = null!;

		// Payment Information
		public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>();

		// Review Information
		public bool CanReview { get; set; }
		public bool HasReviewed { get; set; }

		public DateTime? PaymentExpiresAt { get; set; }
	}
}
