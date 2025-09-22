namespace BookingSystem.Application.Models.Responses
{
	public class PagedResponse<T> : BaseResponse
	{
		public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
		public PaginationMetadata Pagination { get; set; } = new();

		public PagedResponse()
		{
		}

		public PagedResponse(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize)
		{
			Data = data;
			Success = true;
			Message = "Success";
			Pagination = new PaginationMetadata
			{
				CurrentPage = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
				HasNextPage = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize),
				HasPreviousPage = pageNumber > 1
			};
		}
	}
}
