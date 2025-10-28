using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Application.DTOs.HomestayImageDTO;
using BookingSystem.Application.DTOs.HomestayRuleDTO;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.AccommodationDTO
{
	public class CreateHomestayDto
	{
		[Required(ErrorMessage = "Homestay title is required.")]
		[MaxLength(200, ErrorMessage = "Homestay title cannot exceed 200 characters.")]
		public string HomestayTitle { get; set; } = string.Empty;

		[MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
		public string? HomestayDescription { get; set; }

		public string? Slug { get; set; }

		[Required(ErrorMessage = "Full address is required.")]
		[MaxLength(300, ErrorMessage = "Full address cannot exceed 300 characters.")]
		public string FullAddress { get; set; } = string.Empty;

		[Required(ErrorMessage = "City is required.")]
		[MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
		public string City { get; set; } = string.Empty;

		[Required(ErrorMessage = "Province is required.")]
		[MaxLength(100, ErrorMessage = "Province cannot exceed 100 characters.")]
		public string Province { get; set; } = string.Empty;

		[Required(ErrorMessage = "Country is required.")]
		[MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
		public string Country { get; set; } = "Vietnam";

		[MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters.")]
		public string? PostalCode { get; set; }

		[Required(ErrorMessage = "Latitude is required.")]
		[Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
		public decimal Latitude { get; set; }

		[Required(ErrorMessage = "Longitude is required.")]
		[Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
		public decimal Longitude { get; set; }

		[Range(0, 10000, ErrorMessage = "Area must be a positive value.")]
		public decimal? AreaInSquareMeters { get; set; }

		[Range(0, 100, ErrorMessage = "Number of floors must be between 0 and 100.")]
		public int? NumberOfFloors { get; set; }

		[Range(0, 100, ErrorMessage = "Number of rooms must be between 0 and 100.")]
		public int NumberOfRooms { get; set; }

		[Required(ErrorMessage = "Number of bedrooms is required.")]
		[Range(0, 50, ErrorMessage = "Number of bedrooms must be between 0 and 50.")]
		public int NumberOfBedrooms { get; set; }

		[Required(ErrorMessage = "Number of bathrooms is required.")]
		[Range(0, 50, ErrorMessage = "Number of bathrooms must be between 0 and 50.")]
		public int NumberOfBathrooms { get; set; }

		[Required(ErrorMessage = "Number of beds is required.")]
		[Range(0, 50, ErrorMessage = "Number of beds must be between 0 and 50.")]
		public int NumberOfBeds { get; set; }

		[Required(ErrorMessage = "Maximum guests is required.")]
		[Range(1, 50, ErrorMessage = "Maximum guests must be between 1 and 50.")]
		public int MaximumGuests { get; set; }

		[Range(0, 50, ErrorMessage = "Maximum children must be between 0 and 50.")]
		public int MaximumChildren { get; set; }

		[Required(ErrorMessage = "Base nightly price is required.")]
		[Range(0, 1000000000, ErrorMessage = "Base nightly price must be greater than 0.")]
		public decimal BaseNightlyPrice { get; set; }

		[Range(0, 1000000000, ErrorMessage = "Weekend price must be greater than or equal to 0.")]
		public decimal? WeekendPrice { get; set; }

		[Range(0, 100, ErrorMessage = "Weekly discount must be between 0% and 100%.")]
		public decimal? WeeklyDiscount { get; set; }

		[Range(0, 100, ErrorMessage = "Monthly discount must be between 0% and 100%.")]
		public decimal? MonthlyDiscount { get; set; }

		[Required(ErrorMessage = "Minimum nights is required.")]
		[Range(1, 30, ErrorMessage = "Minimum nights must be between 1 and 30.")]
		public int MinimumNights { get; set; } = 1;

		[Required(ErrorMessage = "Maximum nights is required.")]
		[Range(1, 365, ErrorMessage = "Maximum nights must be between 1 and 365.")]
		public int MaximumNights { get; set; } = 365;

		public bool? IsFreeCancellation { get; set; }   
		public int? FreeCancellationDays { get; set; }     
		public bool? IsPrepaymentRequired { get; set; }  

		public int? AvailableRooms { get; set; }           
		public int? RoomsAtThisPrice { get; set; }         

		public TimeOnly CheckInTime { get; set; } = new TimeOnly(15, 0);
		public TimeOnly CheckOutTime { get; set; } = new TimeOnly(11, 0);
		public bool IsInstantBook { get; set; } = false;
		public bool HasParking { get; set; } = false;
		public bool IsPetFriendly { get; set; } = false;
		public bool HasPrivatePool { get; set; } = false;

		[Required(ErrorMessage = "OwnerId is required.")]
		public int OwnerId { get; set; }

		[Required(ErrorMessage = "PropertyTypeId is required.")]
		public int PropertyTypeId { get; set; }
		public string? SearchKeywords { get; set; }
		[Required(ErrorMessage = "At least one image is required.")]
		[MinLength(1, ErrorMessage = "At least one image is required.")]
		public List<CreateHomestayImageDto> Images { get; set; } = new();

		public List<CreateHomestayAmenityDto> Amenities { get; set; } = new();
		public List<CreateHomestayRuleDto> Rules { get; set; } = new();
		public List<CreateAvailabilityCalendarDto> AvailabilityCalendars { get; set; } = new();
	}
}
