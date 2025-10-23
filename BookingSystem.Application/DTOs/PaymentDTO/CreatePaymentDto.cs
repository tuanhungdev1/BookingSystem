using BookingSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.PaymentDTO
{
	public class CreatePaymentDto
	{
		[Required(ErrorMessage = "Booking ID is required")]
		public int BookingId { get; set; }

		[Required(ErrorMessage = "Payment amount is required")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
		public decimal PaymentAmount { get; set; }

		[Required(ErrorMessage = "Payment method is required")]
		public PaymentMethod PaymentMethod { get; set; }

		[MaxLength(500, ErrorMessage = "Payment notes cannot exceed 500 characters")]
		public string? PaymentNotes { get; set; }
	}
}
