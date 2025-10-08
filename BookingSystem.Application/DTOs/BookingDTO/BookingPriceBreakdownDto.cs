namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class BookingPriceBreakdownDto
	{
		public int NumberOfNights { get; set; }
		public decimal PricePerNight { get; set; }
		public decimal BaseAmount { get; set; }
		public decimal CleaningFee { get; set; }
		public decimal ServiceFee { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal TotalAmount { get; set; }
		public List<NightlyPriceDto> NightlyPrices { get; set; } = new List<NightlyPriceDto>();
	}
}
