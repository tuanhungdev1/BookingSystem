namespace BookingSystem.Domain.Base
{
	public class PagedResult<T>
	{
		public IEnumerable<T> Items { get; set; } = new List<T>();
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public bool HasPreviousPage => PageNumber > 1;
		public bool HasNextPage => PageNumber < TotalPages;
		public int FirstRowOnPage => (PageNumber - 1) * PageSize + 1;
		public int LastRowOnPage => Math.Min(PageNumber * PageSize, TotalCount);

		// Constructor mặc định
		public PagedResult() { }

		// Constructor đầy đủ
		public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
		{
			Items = items ?? new List<T>();
			TotalCount = totalCount;
			PageNumber = pageNumber;
			PageSize = pageSize;
			TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
		}
	}
}
