using BookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Filter
{
	public class BookingFilter : PaginationFilter
	{
		public string? BookingCode { get; set; }
		public int? HomestayId { get; set; }
		public int? GuestId { get; set; }
		public int? HostId { get; set; }
		public BookingStatus? Status { get; set; }
		public DateTime? CheckInDateFrom { get; set; }
		public DateTime? CheckInDateTo { get; set; }
		public DateTime? CheckOutDateFrom { get; set; }
		public DateTime? CheckOutDateTo { get; set; }
		public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }
		public string? SortBy { get; set; } = "CreatedAt";
		public string? SortDirection { get; set; } = "desc"; // "asc" or "desc"
	}
}
