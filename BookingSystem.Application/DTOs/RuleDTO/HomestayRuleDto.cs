namespace BookingSystem.Application.DTOs.RuleDTO
{
	public class HomestayRuleDto
	{
		public int HomestayId { get; set; }
		public int RuleId { get; set; }
		public string? CustomNote { get; set; }
		public DateTime AssignedAt { get; set; }
		public RuleDto Rule { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
