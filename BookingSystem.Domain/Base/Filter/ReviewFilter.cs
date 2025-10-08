using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Filter
{
	public class ReviewFilter : PaginationFilter
	{
		public string? SearchTerm { get; set; }
		public int? HomestayId { get; set; }
		public int? ReviewerId { get; set; }
		public int? RevieweeId { get; set; }
		public int? BookingId { get; set; }
		public int? MinOverallRating { get; set; }
		public int? MaxOverallRating { get; set; }
		public bool? IsVisible { get; set; }
		public bool? IsRecommended { get; set; }
		public bool? HasHostResponse { get; set; }
		public DateTime? CreatedFrom { get; set; }
		public DateTime? CreatedTo { get; set; }
		public string? SortBy { get; set; } = "CreatedAt";// "Date", "Rating", "Helpful"
		public string? SortDirection { get; set; } = "asc";
	}
}
