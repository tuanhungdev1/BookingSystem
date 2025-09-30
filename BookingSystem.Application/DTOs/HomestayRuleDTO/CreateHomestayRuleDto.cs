using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.HomestayRuleDTO
{
	public class CreateHomestayRuleDto
	{
		[Required(ErrorMessage = "Rule ID is required.")]
		public int RuleId { get; set; }

		[MaxLength(500)]
		public string? CustomNote { get; set; }
	}
}
