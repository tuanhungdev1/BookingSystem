using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class Rule : BaseEntity
	{
		public string RuleName { get; set; } = string.Empty;

		public string? RuleDescription { get; set; }

		public string? IconUrl { get; set; }

		public string RuleType { get; set; } = string.Empty; // Allowed, NotAllowed, Required

		public bool IsActive { get; set; } = true;

		public int DisplayOrder { get; set; } = 0;

		// Navigation Properties
		public virtual ICollection<HomestayRule> HomestayRules { get; set; } = new List<HomestayRule>();
	}
}
