using BookingSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.UserDTO
{
	public class UpdateUserDto
	{
		[Required(ErrorMessage = "First name is required")]
		[StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Last name is required")]
		[StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
		public string LastName { get; set; } = string.Empty;

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

		public string? PhoneNumber { get; set; }

		[StringLength(200, ErrorMessage = "Avatar URL cannot exceed 200 characters")]
		[Url(ErrorMessage = "Invalid URL format")]
		public string? Avatar { get; set; }
	}
}
