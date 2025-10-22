using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Application.DTOs.HomestayDTO;
using BookingSystem.Application.DTOs.PropertyTypeDTO;
using BookingSystem.Application.DTOs.RuleDTO;
using BookingSystem.Application.DTOs.UserDTO;

namespace BookingSystem.Application.DTOs.AccommodationDTO
{
	public class HomestayDto
	{
		public int Id { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string? HomestayDescription { get; set; }

		// Address
		public string FullAddress { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Province { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
		public string? PostalCode { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }

		// Capacity
		public int MaximumGuests { get; set; }
		public int NumberOfBedrooms { get; set; }
		public int NumberOfBathrooms { get; set; }
		public int NumberOfBeds { get; set; }

		// Pricing
		public decimal BaseNightlyPrice { get; set; }
		public decimal? WeekendPrice { get; set; }
		public decimal? WeeklyDiscount { get; set; }
		public decimal? MonthlyDiscount { get; set; }

		// Booking Rules
		public int MinimumNights { get; set; }
		public int MaximumNights { get; set; }
		public TimeOnly CheckInTime { get; set; }
		public TimeOnly CheckOutTime { get; set; }
		public bool IsInstantBook { get; set; }

		// Status
		public bool IsActive { get; set; }
		public bool IsApproved { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public string? ApprovedBy { get; set; }
		public bool IsFeatured { get; set; }

		// Owner Info
		public string OwnerId { get; set; } = string.Empty;
		public string OwnerName { get; set; } = string.Empty;
		public string OwnerPhone { get; set; } = string.Empty;
		public string OwnerEmail { get; set; } = string.Empty;
		public string? OwnerAvatar { get; set; }

		// Property Type
		public int PropertyTypeId { get; set; }
		public string PropertyTypeName { get; set; } = string.Empty;
		public string? PropertyTypeIcon { get; set; }

		// Images
		public string? MainImageUrl { get; set; }
		public List<HomestayImageDto> Images { get; set; } = new();

		// Amenities - CHỈ GỬI THÔNG TIN AMENITY, KHÔNG GỬI BẢNG TRUNG GIAN
		public List<AmenitySimpleDto> Amenities { get; set; } = new();

		// Rules - CHỈ GỬI THÔNG TIN RULE, KHÔNG GỬI BẢNG TRUNG GIAN
		public List<RuleSimpleDto> Rules { get; set; } = new();

		// Availability Calendars
		public List<AvailabilityCalendarDto> AvailabilityCalendars { get; set; } = new();

		// Statistics
		public double AverageRating { get; set; }
		public int TotalReviews { get; set; }
		public int TotalBookings { get; set; }

		// Timestamps
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
