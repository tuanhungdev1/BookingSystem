using BookingSystem.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Domain.Entities
{
	public class HomestayRule : BaseEntity
	{
		public int HomestayId { get; set; }
		public int RuleId { get; set; }

		[MaxLength(500)]
		public string? CustomNote { get; set; }

		public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

		// Navigation Properties
		public virtual Homestay Homestay { get; set; } = null!;
		public virtual Rule Rule { get; set; } = null!;
	}
}
