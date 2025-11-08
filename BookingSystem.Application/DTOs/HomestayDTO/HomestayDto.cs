using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Application.DTOs.HomestayDTO;
using BookingSystem.Application.DTOs.HomestayImageDTO;
using BookingSystem.Application.DTOs.RuleDTO;

namespace BookingSystem.Application.DTOs.AccommodationDTO
{
	public class HomestayDto
	{

		public int Id { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string? HomestayDescription { get; set; }
		public string? Slug { get; set; }


		public string FullAddress { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Province { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
		public string? PostalCode { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }

	
		public decimal? AreaInSquareMeters { get; set; }    // Diện tích
		public int? NumberOfFloors { get; set; }            // Số tầng
		public int NumberOfRooms { get; set; }              // Tổng số phòng
		public int NumberOfBedrooms { get; set; }
		public int NumberOfBathrooms { get; set; }
		public int NumberOfBeds { get; set; }


		public int MaximumGuests { get; set; }
		public int MaximumChildren { get; set; }


		public decimal BaseNightlyPrice { get; set; }
		public decimal? WeekendPrice { get; set; }
		public decimal? WeeklyDiscount { get; set; }
		public decimal? MonthlyDiscount { get; set; }

		public int MinimumNights { get; set; }
		public int MaximumNights { get; set; }
		public TimeOnly CheckInTime { get; set; }
		public TimeOnly CheckOutTime { get; set; }
		public bool IsInstantBook { get; set; }

		public bool IsFreeCancellation { get; set; }   // Hủy miễn phí
		public int FreeCancellationDays { get; set; }     // Hủy miễn phí trước X ngày
		public bool IsPrepaymentRequired { get; set; }  // Có cần thanh toán trước không

		/* =============================
		 * 🏨 Tình trạng phòng (Availability Info)
		 * ============================= */
		public int AvailableRooms { get; set; }           // Số phòng còn trống
		public int RoomsAtThisPrice { get; set; }         // Còn X phòng với giá hiện tại

		public bool IsActive { get; set; }
		public bool IsApproved { get; set; }
		public bool IsFeatured { get; set; }
		public string? ApprovalNote { get; set; }
		public string? RejectionReason { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public string? ApprovedBy { get; set; }

		public string? SearchKeywords { get; set; }
		public int ViewCount { get; set; }
		public double RatingAverage { get; set; } = 0.0;
		public int TotalReviews { get; set; } = 0;
		public int BookingCount { get; set; }


		public bool HasParking { get; set; }
		public bool IsPetFriendly { get; set; }
		public bool HasPrivatePool { get; set; }


		public int OwnerId { get; set; }
		public string OwnerName { get; set; } = string.Empty;
		public string OwnerPhone { get; set; } = string.Empty;
		public string OwnerEmail { get; set; } = string.Empty;
		public string? OwnerAvatar { get; set; }


		public int PropertyTypeId { get; set; }
		public string PropertyTypeName { get; set; } = string.Empty;
		public string? PropertyTypeIcon { get; set; }


		public string? MainImageUrl { get; set; }
		public List<HomestayImageDto> Images { get; set; } = new();
		public List<AmenitySimpleDto> Amenities { get; set; } = new();
		public List<RuleSimpleDto> Rules { get; set; } = new();

		public List<AvailabilityCalendarDto> AvailabilityCalendars { get; set; } = new();

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
