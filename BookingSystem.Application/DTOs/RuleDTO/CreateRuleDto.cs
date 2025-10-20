using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.RuleDTO
{
	public class CreateRuleDto
	{
		[Required(ErrorMessage = "Rule name is required")]
		[MaxLength(100, ErrorMessage = "Rule name cannot exceed 100 characters")]
		public string RuleName { get; set; } = string.Empty;

		[MaxLength(500, ErrorMessage = "Rule description cannot exceed 500 characters")]
		public string? RuleDescription { get; set; }

		[Required(ErrorMessage = "IconFile is required")]
		public IFormFile? IconFile { get; set; }

		[Required(ErrorMessage = "Rule type is required")]
		public string RuleType { get; set; } = string.Empty;

		public bool IsActive { get; set; } = true;

		[Range(0, int.MaxValue, ErrorMessage = "Display order must be a non-negative number")]
		public int DisplayOrder { get; set; } = 0;
	}
}
