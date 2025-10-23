using BookingSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.UserDTO
{
	public class CreateUserDto
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		[MaxLength(256, ErrorMessage = "Email must not exceed 256 characters.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "First name is required.")]
		[MaxLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Last name is required.")]
		[MaxLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
		public string LastName { get; set; } = string.Empty;

		[DataType(DataType.Date, ErrorMessage = "Invalid date of birth format.")]
		public DateTime? DateOfBirth { get; set; }

		public Gender? Gender { get; set; }

		[MaxLength(255, ErrorMessage = "Address must not exceed 255 characters.")]
		public string? Address { get; set; }

		[MaxLength(100, ErrorMessage = "City name must not exceed 100 characters.")]
		public string? City { get; set; }

		[MaxLength(100, ErrorMessage = "Country name must not exceed 100 characters.")]
		public string? Country { get; set; }

		[MaxLength(20, ErrorMessage = "Postal code must not exceed 20 characters.")]
		public string? PostalCode { get; set; }

		[Phone(ErrorMessage = "Invalid phone number format.")]
		public string? PhoneNumber { get; set; }

		public IList<string>? Roles { get; set; }
	}
}
