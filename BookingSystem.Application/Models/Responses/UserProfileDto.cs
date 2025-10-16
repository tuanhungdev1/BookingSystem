using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.Models.Responses
{
	public class UserProfileDto
	{
		public int Id { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string FullName => $"{FirstName} {LastName}".Trim();
		public string? PhoneNumber { get; set; }
		public string? Avatar { get; set; }
		public Gender? Gender { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? Address { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }
		public bool IsEmailConfirmed { get; set; }
		public bool TwoFactorEnabled { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}
}
