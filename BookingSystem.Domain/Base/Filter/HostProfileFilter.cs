using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Base.Filter
{
    public class HostProfileFilter : PaginationFilter
    {
		public string? SearchTerm { get; set; }

		public bool? IsActive { get; set; }

		public HostStatus? HostStatus { get; set; }

		public bool? IsSuperhost { get; set; }

		public DateTime? RegisteredFrom { get; set; }
		public DateTime? RegisteredTo { get; set; }

		public DateTime? ReviewedFrom { get; set; }
		public DateTime? ReviewedTo { get; set; }

		public decimal? MinAverageRating { get; set; }
		public decimal? MaxAverageRating { get; set; }

		public int? MinTotalBookings { get; set; }
		public int? MaxTotalBookings { get; set; }

		public int? MinTotalHomestays { get; set; }
		public int? MaxTotalHomestays { get; set; }

		public int? MinResponseRate { get; set; }
		public int? MaxResponseRate { get; set; }

		public string? SortBy { get; set; } = "RegisteredAsHostAt"; // Default sort by RegisteredAsHostAt
		public string? SortDirection { get; set; } = "desc"; // Default descending
	}
}
