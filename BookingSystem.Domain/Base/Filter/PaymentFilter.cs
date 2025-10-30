using BookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Base.Filter
{
	public class PaymentFilter : PaginationFilter
	{
		public string? Search { get; set; }
		public string? SortBy { get; set; }  // "createdAt" | "paymentAmount" | "processedAt"
		public string? SortDirection { get; set; }  // "asc" | "desc"

		public PaymentStatus? PaymentStatus { get; set; }
		public PaymentMethod? PaymentMethod { get; set; }

		public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }

		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }

		public string? BookingCode { get; set; }
	}
}
