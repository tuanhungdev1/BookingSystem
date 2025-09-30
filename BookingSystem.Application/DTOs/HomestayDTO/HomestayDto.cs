using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.AccommodationDTO
{
	public class HomestayDto
	{
		public int Id { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string? HomestayDescription { get; set; }
		public string FullAddress { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Province { get; set; } = string.Empty;
		public string Country { get; set; } = "Vietnam";
		public string? PostalCode { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public int MaximumGuests { get; set; }
		public int NumberOfBedrooms { get; set; }
		public int NumberOfBathrooms { get; set; }
		public int NumberOfBeds { get; set; }
		public decimal BaseNightlyPrice { get; set; }
		public decimal? WeekendPrice { get; set; }
		public decimal? WeeklyDiscount { get; set; }
		public decimal? MonthlyDiscount { get; set; }
		public int MinimumNights { get; set; }
		public int MaximumNights { get; set; }
		public TimeOnly CheckInTime { get; set; }
		public TimeOnly CheckOutTime { get; set; }
		public bool IsInstantBook { get; set; }
		public bool IsActive { get; set; }
		public bool IsApproved { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public string? ApprovedBy { get; set; }
		public bool IsFeatured { get; set; }

		// Foreign Keys
		public string OwnerId { get; set; } = string.Empty;
		public int PropertyTypeId { get; set; }

		// Extra info để trả về UI
		public string OwnerName { get; set; } = string.Empty;
		public string PropertyTypeName { get; set; } = string.Empty;
		public List<string> ImageUrls { get; set; } = new();
		public List<string> Amenities { get; set; } = new();
		public List<string> Rules { get; set; } = new();
		public double AverageRating { get; set; }
	}
}
