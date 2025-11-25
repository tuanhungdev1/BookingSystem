using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.ExportDTOs
{
	public class AmenityExportDto
	{
		public int Id { get; set; }
		public string AmenityName { get; set; } = string.Empty;
		public string? AmenityDescription { get; set; }
		public string Category { get; set; } = string.Empty;
		public bool IsCommon { get; set; }
		public int UsageCount { get; set; }
		public bool IsActive { get; set; }
		public int DisplayOrder { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

	// ========== PAYMENT DTO ==========
	public class PaymentExportDto
	{
		public int Id { get; set; }
		public int BookingId { get; set; }
		public string BookingCode { get; set; } = string.Empty;
		public decimal PaymentAmount { get; set; }
		public string PaymentMethod { get; set; } = string.Empty;
		public string PaymentStatus { get; set; } = string.Empty;
		public string? TransactionId { get; set; }
		public string? PaymentGateway { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? PaymentNotes { get; set; }
		public decimal? RefundAmount { get; set; }
		public DateTime? RefundedAt { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	// ========== PROPERTY TYPE DTO ==========
	public class PropertyTypeExportDto
	{
		public int Id { get; set; }
		public string TypeName { get; set; } = string.Empty;
		public string? Description { get; set; }
		public bool IsActive { get; set; }
		public int DisplayOrder { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

	// ========== BOOKING DTO ==========
	public class BookingExportDto
	{
		public int Id { get; set; }
		public string BookingCode { get; set; } = string.Empty;
		public int GuestId { get; set; }
		public string GuestFullName { get; set; } = string.Empty;
		public string GuestEmail { get; set; } = string.Empty;
		public string GuestPhoneNumber { get; set; } = string.Empty;
		public int HomestayId { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public int NumberOfGuests { get; set; }
		public int NumberOfAdults { get; set; }
		public int NumberOfChildren { get; set; }
		public decimal BaseAmount { get; set; }
		public decimal CleaningFee { get; set; }
		public decimal ServiceFee { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal TotalAmount { get; set; }
		public string BookingStatus { get; set; } = string.Empty;
		public bool IsBookingForSomeoneElse { get; set; }
		public string? ActualGuestFullName { get; set; }
		public string? ActualGuestPhoneNumber { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? CancelledAt { get; set; }
	}
}
