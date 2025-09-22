using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Domain.Entities
{
	public class Role : IdentityRole<Guid>
	{
		public string? Description { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		// Navigation Properties
		public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
	}
}
