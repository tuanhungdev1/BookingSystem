using BookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
