using BookingSystem.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Domain.Entities
{
	public class Homestay : BaseEntity
	{
		public string HomestayTitle { get; set; } = string.Empty;
		public string? HomestayDescription { get; set; }
		public string? Slug { get; set; }

		public string FullAddress { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Province { get; set; } = string.Empty;
		public string Country { get; set; } = "Vietnam";
		public string? PostalCode { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }

		public decimal? AreaInSquareMeters { get; set; }    // Diện tích
		public int? NumberOfFloors { get; set; }            // Số tầng
		public int NumberOfRooms { get; set; }              // Tổng số phòng
		public int NumberOfBedrooms { get; set; }
		public int NumberOfBathrooms { get; set; }
		public int NumberOfBeds { get; set; }

		public int MaximumGuests { get; set; }              // Tổng khách tối đa
		public int MaximumChildren { get; set; }            // Trẻ em tối đa

		public decimal BaseNightlyPrice { get; set; }
		public decimal? WeekendPrice { get; set; }
		public decimal? WeeklyDiscount { get; set; }
		public decimal? MonthlyDiscount { get; set; }

		public int MinimumNights { get; set; } = 1;
		public int MaximumNights { get; set; } = 365;
		public TimeOnly CheckInTime { get; set; } = new TimeOnly(15, 0);
		public TimeOnly CheckOutTime { get; set; } = new TimeOnly(11, 0);
		public bool IsInstantBook { get; set; } = false;
		public bool IsFreeCancellation { get; set; } = false;   // Hủy miễn phí
		public int FreeCancellationDays { get; set; } = 0;      // Hủy miễn phí trước X ngày
		public bool IsPrepaymentRequired { get; set; } = true;  // Có cần thanh toán trước không

		public int AvailableRooms { get; set; } = 0;            // Số phòng còn trống
		public int RoomsAtThisPrice { get; set; } = 0;          // Còn X phòng với giá hiện tại

		public bool IsActive { get; set; } = true;
		public bool IsApproved { get; set; } = false;
		public bool IsFeatured { get; set; } = false;
		public string? ApprovalNote { get; set; }
		public string? RejectionReason { get; set; }
		public DateTime? ApprovedAt { get; set; }
		[MaxLength(100)]
		public string? ApprovedBy { get; set; }

		public string? SearchKeywords { get; set; }
		public int ViewCount { get; set; } = 0;
		public double RatingAverage { get; set; } = 0.0;
		public int TotalReviews { get; set; } = 0;
		public int BookingCount { get; set; } = 0;

		public bool HasParking { get; set; } = false;
		public bool IsPetFriendly { get; set; } = false;
		public bool HasPrivatePool { get; set; } = false;

		public string? HomestayTitleNormalized { get; set; }
		public string? HomestayDescriptionNormalized { get; set; }
		public string? FullAddressNormalized { get; set; }
		public string? CountryNormalized { get; set; }
		public string? CityNormalized { get; set; }
		public string? ProvinceNormalized { get; set; }
		public string? SearchKeywordsNormalized { get; set; }
		public string? SlugNormalized { get; set; }

		[Required]
		public int OwnerId { get; set; }
		public int PropertyTypeId { get; set; }

		public virtual User Owner { get; set; } = null!;
		public virtual PropertyType PropertyType { get; set; } = null!;
		public virtual ICollection<HomestayImage> HomestayImages { get; set; } = new List<HomestayImage>();
		public virtual ICollection<HomestayAmenity> HomestayAmenities { get; set; } = new List<HomestayAmenity>();
		public virtual ICollection<HomestayRule> HomestayRules { get; set; } = new List<HomestayRule>();
		public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
		public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
		public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
		public virtual ICollection<AvailabilityCalendar> AvailabilityCalendars { get; set; } = new List<AvailabilityCalendar>();
	}
}
