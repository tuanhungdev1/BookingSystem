namespace BookingSystem.Domain.Base.Filter
{
    public class AmenityFilter : PaginationFilter
	{
		public string? Search { get; set; }
		public string? Category { get; set; }
		public bool? IsActive { get; set; }
		public string? SortBy { get; set; } = "createdAt";
		public string? SortOrder { get; set; } = "asc";
	}
}
