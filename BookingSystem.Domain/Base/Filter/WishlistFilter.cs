using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Filter
{
	public class WishlistFilter : PaginationFilter
	{
		public string? HomestayName { get; set; }
		public string? Location { get; set; }
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
		public int? PropertyTypeId { get; set; }
		
		public string? sortBy { get; set; } = "CreatedAt"; // Price, CreatedAt, Rating
		public string? sortOrder { get; set; } = "desc"; // asc, desc
	}
}
