using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Filter
{
    public class RuleFilter : PaginationFilter
    {
		public string? RuleName { get; set; }
		public string? RuleType { get; set; }
		public bool? IsActive { get; set; }
		public string? SortBy { get; set; } = "RuleName"; 
		public string? SortOrder { get; set; } = "asc"; 
	}
}
