using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Domain.Entities
{
	public class UserRole : IdentityUserRole<Guid>
	{
		public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
		public string? AssignedBy { get; set; }

		// Navigation Properties
		public virtual User User { get; set; } = null!;
		public virtual Role Role { get; set; } = null!;
	}
}
