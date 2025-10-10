using BookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.PaymentDTO
{
	public class CreateOnlinePaymentDto
	{
		[Required(ErrorMessage = "Booking ID is required")]
		public int BookingId { get; set; }

		[Required(ErrorMessage = "Payment method is required")]
		public PaymentMethod PaymentMethod { get; set; }

		[Required(ErrorMessage = "Return URL is required")]
		public string ReturnUrl { get; set; } = string.Empty;

		[MaxLength(500, ErrorMessage = "Payment notes cannot exceed 500 characters")]
		public string? PaymentNotes { get; set; }
	}
}
