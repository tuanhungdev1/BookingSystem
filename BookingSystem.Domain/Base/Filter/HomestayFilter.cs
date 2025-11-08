namespace BookingSystem.Domain.Base.Filter
{
	public class HomestayFilter : PaginationFilter
	{
		// Existing filters
		public string? Search { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }
		public string? Type { get; set; }
		public bool? IsActive { get; set; }

		// NEW: Additional filters
		public string? Province { get; set; }
		public bool? IsApproved { get; set; }
		public bool? IsFeatured { get; set; }
		public bool? IsInstantBook { get; set; }

		// Price range
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
		public bool? HasWeekendPrice { get; set; }
		public bool? HasWeeklyDiscount { get; set; }
		public bool? HasMonthlyDiscount { get; set; }

		// Guest capacity
		public int? MinGuests { get; set; }
		public int? MinChildren { get; set; }

		// Rooms
		public int? MinBedrooms { get; set; }
		public int? MinBathrooms { get; set; }
		public int? MinBeds { get; set; }
		public int? MinRooms { get; set; }

		// Location (radius search)
		public decimal? Latitude { get; set; }
		public decimal? Longitude { get; set; }
		public decimal? RadiusInKm { get; set; }

		// ✅ Amenities / features
		public bool? HasParking { get; set; }
		public bool? IsPetFriendly { get; set; }
		public bool? HasPrivatePool { get; set; }


		// Owner filter
		public int? OwnerId { get; set; }

		// Property type filter
		public int? PropertyTypeId { get; set; }

		// Date range filter (for availability)
		public DateOnly? CheckInDate { get; set; }
		public DateOnly? CheckOutDate { get; set; }

		
		public string? AmenityIds { get; set; }
		
		public string? PropertyTypeIds { get; set; } 

		public int? Adults { get; set; }
		public int? Children { get; set; }
		public int? Rooms { get; set; }
		public bool? Pets { get; set; }

		public double? MinRating { get; set; }

		public DateTime? CreatedFrom { get; set; }
		public DateTime? CreatedTo { get; set; }
		public DateTime? ApprovedFrom { get; set; }
		public DateTime? ApprovedTo { get; set; }

		public string? SortBy { get; set; } = "CreatedAt";
		public string? SortDirection { get; set; } = "desc";
	}
}
