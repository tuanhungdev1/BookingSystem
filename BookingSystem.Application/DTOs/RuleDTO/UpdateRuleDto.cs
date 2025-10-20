using BookingSystem.Application.Models.Constants;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.RuleDTO
{
	public class UpdateRuleDto
	{
		[MaxLength(100, ErrorMessage = "Rule name cannot exceed 100 characters")]
		public string? RuleName { get; set; }

		[MaxLength(500, ErrorMessage = "Rule description cannot exceed 500 characters")]
		public string? RuleDescription { get; set; }

		public IFormFile? IconFile { get; set; }
		public ImageAction ImageAction { get; set; } = ImageAction.Keep;
		public string? RuleType { get; set; }

		public bool? IsActive { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "Display order must be a non-negative number")]
		public int? DisplayOrder { get; set; }
	}
}
