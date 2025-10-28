using BookingSystem.Application.DTOs.HomestayRuleDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.RuleDTO
{
	public class UpdateHomestayRulesDto
	{
		[Required]
		public List<int> KeepRuleIds { get; set; } = new List<int>();

		public List<CreateHomestayRuleDto> NewRules { get; set; } = new List<CreateHomestayRuleDto>();

		public List<UpdateHomestayRuleDto>? UpdateExistingRules { get; set; } = new List<UpdateHomestayRuleDto>();
	}
}
