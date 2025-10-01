namespace BookingSystem.Domain.Base.Filter
{
    public class AmenityFilter : PaginationFilter
	{
		public string? SearchTerm { get; set; }
		public string? Category { get; set; }
		public bool? IsPopular { get; set; }
		public bool? IsActive { get; set; }
		public string? SortBy { get; set; } = "name";
		public string? SortDirection { get; set; } = "asc";
	}
}
