using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.RuleDTO
{
	public class UpdateHomestayRuleDto
	{
		[Required]
		public int RuleId { get; set; }

		[MaxLength(500)]
		public string? CustomNote { get; set; }
	}
}
