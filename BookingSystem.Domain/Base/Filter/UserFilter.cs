namespace BookingSystem.Domain.Base.Filter
{
    public class UserFilter : PaginationFilter
	{
		public string? SearchTerm { get; set; }
		public bool? IsActive { get; set; }
		public string? SortBy { get; set; } = "name";
		public string? SortDirection { get; set; } = "asc";
	}
}
