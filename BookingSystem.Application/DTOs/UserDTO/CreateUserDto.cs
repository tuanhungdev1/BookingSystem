using BookingSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.UserDTO
{
	public class CreateUserDto
	{
		[Required(ErrorMessage = "First name is required")]
		[StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Last name is required")]
		[StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
		public string LastName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email address format")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "Date of birth is required")]
		[DataType(DataType.Date, ErrorMessage = "Invalid date format")]
		public DateTime? DateOfBirth { get; set; }

		public Gender? Gender { get; set; }

		[StringLength(100, ErrorMessage = "Address cannot exceed 100 characters")]
		public string? Address { get; set; }

		[StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
		public string? City { get; set; }

		[StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
		public string? Country { get; set; }

		[StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
		[RegularExpression(@"^[A-Za-z0-9\s-]*$", ErrorMessage = "Invalid postal code format")]
		public string? PostalCode { get; set; }

		public string? PhoneNumber { get; set; } = string.Empty;
		public bool? IsEmailConfirmed { get; set; } = false;
		public bool IsActive { get; set; } = false;

		[StringLength(200, ErrorMessage = " StuAvatar URL cannot exceed 200 characters")]
		[Url(ErrorMessage = "Invalid URL format")]
		public string? Avatar { get; set; }
		public IEnumerable<string> Roles { get; set; } = new List<string> { "Customer" };
	}
}
