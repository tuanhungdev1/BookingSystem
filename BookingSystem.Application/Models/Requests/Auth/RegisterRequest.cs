using BookingSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class RegisterRequest
	{
		[Required(ErrorMessage = "First name is required")]
		[StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Last name is required")]
		[StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
		public string LastName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		[MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
			ErrorMessage = "Password must contain at least one uppercase, one lowercase, one digit, and one special character.")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password confirmation is required")]
		[Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
		public string ConfirmPassword { get; set; } = string.Empty;

		[Phone(ErrorMessage = "Invalid phone number format")]
		public string? PhoneNumber { get; set; }

		public Gender? Gender { get; set; }

		[DataType(DataType.Date)]
		public DateTime? DateOfBirth { get; set; }

		public string? Address { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }

		[Required(ErrorMessage = "You must accept the terms and conditions")]
		public bool AcceptTerms { get; set; }
	}
}
