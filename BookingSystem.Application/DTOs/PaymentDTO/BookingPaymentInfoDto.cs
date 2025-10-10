using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.PaymentDTO
{
	public class BookingPaymentInfoDto
	{
		public int Id { get; set; }
		public string BookingCode { get; set; } = string.Empty;
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string GuestName { get; set; } = string.Empty;
	}
}
