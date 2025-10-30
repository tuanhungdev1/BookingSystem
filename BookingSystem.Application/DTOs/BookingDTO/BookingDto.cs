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

		// Guest Information
		public int GuestId { get; set; }
		public string GuestName { get; set; } = string.Empty;
		public string GuestEmail { get; set; } = string.Empty;
		public string? GuestPhone { get; set; }
		public string? GuestAvatar { get; set; }

		// Homestay Information
		public BookingHomestayDto Homestay { get; set; } = null!;

		// Payment Information
		public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>();

		// Review Information
		public bool CanReview { get; set; }
		public bool HasReviewed { get; set; }
	}
}
