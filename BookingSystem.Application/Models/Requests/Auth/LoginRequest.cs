using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class LoginRequest
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		[MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
			ErrorMessage = "Password must contain at least one uppercase, one lowercase, one digit, and one special character.")]
		public string Password { get; set; } = string.Empty;
		public bool RememberMe { get; set; } = false;
	}
}
