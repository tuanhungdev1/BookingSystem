using BookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Filter
{
	public class CouponFilter : PaginationFilter
	{
		public string? SearchTerm { get; set; }
		public string? CouponCode { get; set; }
		public CouponType? CouponType { get; set; }
		public bool? IsActive { get; set; }
		public bool? IsPublic { get; set; }
		public bool? IsExpired { get; set; }
		public DateTime? ValidFrom { get; set; }
		public DateTime? ValidTo { get; set; }
		public int? HomestayId { get; set; }
		public int? CreatedByUserId { get; set; }
		public string? SortBy { get; set; } = "CreatedAt"; // "Priority", "StartDate", "EndDate", "Usage"
		public string? SortDirection { get; set; } = "asc";
	}
}
