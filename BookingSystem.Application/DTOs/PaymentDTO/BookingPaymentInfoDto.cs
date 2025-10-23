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
