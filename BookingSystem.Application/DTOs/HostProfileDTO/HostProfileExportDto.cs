using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.HostProfileDTO
{
	public class HostProfileExportDto
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string BusinessName { get; set; } = string.Empty;
		public string AboutMe { get; set; } = string.Empty;
		public string Languages { get; set; } = string.Empty;
		public string BankName { get; set; } = string.Empty;
		public string BankAccountNumber { get; set; } = string.Empty;
		public string BankAccountName { get; set; } = string.Empty;
		public string TaxCode { get; set; } = string.Empty;
		public int TotalHomestays { get; set; }
		public int TotalBookings { get; set; }
		public decimal AverageRating { get; set; }
		public int ResponseRate { get; set; }
		public string AverageResponseTime { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public bool IsSuperhost { get; set; }
		public DateTime RegisteredAsHostAt { get; set; }
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public string ApplicantNote { get; set; } = string.Empty;
		public string ReviewNote { get; set; } = string.Empty;
	}
}
