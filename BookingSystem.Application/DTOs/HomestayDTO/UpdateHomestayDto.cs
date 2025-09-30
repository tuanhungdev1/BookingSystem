namespace BookingSystem.Application.DTOs.AccommodationDTO
{
	namespace BookingSystem.Application.DTOs
	{
		public class UpdateHomestayDto
		{
			public string? HomestayTitle { get; set; }

			public string? HomestayDescription { get; set; }

			public string? FullAddress { get; set; }

			public string? City { get; set; }

			public string? Province { get; set; }

			public string? Country { get; set; }

			public string? PostalCode { get; set; }

			public decimal? Latitude { get; set; }

			public decimal? Longitude { get; set; }

			public int? MaximumGuests { get; set; }

			public int? NumberOfBedrooms { get; set; }

			public int? NumberOfBathrooms { get; set; }

			public int? NumberOfBeds { get; set; }

			public decimal? BaseNightlyPrice { get; set; }

			public decimal? WeekendPrice { get; set; }

			public decimal? WeeklyDiscount { get; set; }

			public decimal? MonthlyDiscount { get; set; }

			public int? MinimumNights { get; set; }

			public int? MaximumNights { get; set; }

			public int? PropertyTypeId { get; set; }
		}
	}
}
