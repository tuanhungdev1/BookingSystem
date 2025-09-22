namespace BookingSystem.Application.Models.Requests
{
	public class SearchRequest : PaginationRequest
	{
		public string? SearchTerm { get; set; }
		public string? SortBy { get; set; }
		public string SortDirection { get; set; } = "asc";
		public Dictionary<string, object>? Filters { get; set; }

		public bool IsDescending => SortDirection.ToLower() == "desc";
	}
}
