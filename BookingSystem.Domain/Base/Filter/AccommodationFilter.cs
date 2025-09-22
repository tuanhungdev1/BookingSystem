using BookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Filter
{
	public class AccommodationFilter : PaginationFilter
	{
		public string? SearchTerm { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }
		public AccommodationType? Type { get; set; }
		public int? MinStarRating { get; set; }
		public int? MaxStarRating { get; set; }
		public bool? IsActive { get; set; }
		public string? SortBy { get; set; } = "name";
		public string? SortDirection { get; set; } = "asc";
	}
}
