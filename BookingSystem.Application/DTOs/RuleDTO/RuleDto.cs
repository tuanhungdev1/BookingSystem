namespace BookingSystem.Application.DTOs.RuleDTO
{
	public class RuleDto
	{
		public int Id { get; set; }
		public string RuleName { get; set; } = string.Empty;
		public string? RuleDescription { get; set; }
		public string? IconUrl { get; set; }
		public string RuleType { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public int DisplayOrder { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
