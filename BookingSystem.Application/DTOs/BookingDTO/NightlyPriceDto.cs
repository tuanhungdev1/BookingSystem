namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class NightlyPriceDto
	{
		public DateOnly Date { get; set; }
		public decimal Price { get; set; }
		public bool IsWeekend { get; set; }
		public bool IsCustomPrice { get; set; }
	}
}
