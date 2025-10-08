namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class BookingPriceCalculationDto
	{
		public int HomestayId { get; set; }
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public int NumberOfGuests { get; set; }
	}
}
