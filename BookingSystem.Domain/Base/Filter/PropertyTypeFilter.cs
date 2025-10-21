namespace BookingSystem.Domain.Base.Filter
{
    public class PropertyTypeFilter : PaginationFilter
	{
		public string? Search { get; set; }
		public bool? IsActive { get; set; }
		public string? SortBy { get; set; } = "createdAt";
		public string? SortOrder { get; set; } = "asc";
	}
}
