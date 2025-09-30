namespace BookingSystem.Domain.Base.Filter
{
    public class PropertyTypeFilter : PaginationFilter
	{
		public string? SearchTerm { get; set; }
		public bool? IsActive { get; set; }
		public string? SortBy { get; set; } = "name";
		public string? SortDirection { get; set; } = "asc";
	}
}
