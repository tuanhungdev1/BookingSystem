using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.UserDTO
{
	public class UserDto
	{
		public int Id { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string FullName => $"{FirstName} {LastName}";
		public DateTime? DateOfBirth { get; set; }
		public Gender? Gender { get; set; }
		public string? Address { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }
		public string? PostalCode { get; set; }
		public string? Avatar { get; set; }
		public bool IsActive { get; set; }
		public bool IsEmailConfirmed { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsLocked { get; set; }
		public string? PhoneNumber { get; set; }
		// (Tùy chọn) Nếu bạn muốn hiển thị vai trò người dùng:
		public IList<string>? Roles { get; set; }
	}
}
