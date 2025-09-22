using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.AccommodationDTO
{
	public class AccommodationDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
		public string PostalCode { get; set; } = string.Empty;
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public string Phone { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? Website { get; set; }
		public int StarRating { get; set; }
		public string? MainImage { get; set; }
		public AccommodationType Type { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int RoomTypesCount { get; set; }
		public int RoomsCount { get; set; }
	}
}
