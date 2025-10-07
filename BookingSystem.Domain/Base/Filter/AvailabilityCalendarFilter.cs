using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Filter
{
    public class AvailabilityCalendarFilter : PaginationFilter
    {
		public int? HomestayId { get; set; }
		public DateOnly? StartDate { get; set; }
		public DateOnly? EndDate { get; set; }
		public bool? IsAvailable { get; set; }
		public bool? IsBlocked { get; set; }
		public bool? HasCustomPrice { get; set; }
		public string? SortBy { get; set; } = "AvailableDate"; // Default sort by AvailableDate
		public string? SortOrder { get; set; } = "asc"; // Default sort order ascending
	}
}
