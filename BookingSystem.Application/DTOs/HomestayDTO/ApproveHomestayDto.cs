using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.HomestayDTO
{
	public class ApproveHomestayDto
	{
		[Required]
		public bool IsApproved { get; set; }

		[MaxLength(500, ErrorMessage = "Approval note cannot exceed 500 characters")]
		public string? ApprovalNote { get; set; }

		/// <summary>
		/// Auto-activate after approval (default: false)
		/// </summary>
		public bool AutoActivate { get; set; } = false;
	}
}
