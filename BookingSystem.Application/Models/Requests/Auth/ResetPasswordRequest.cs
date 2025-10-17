using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class ResetPasswordRequest
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Reset token is required.")]
		public string Token { get; set; } = string.Empty;

		[Required(ErrorMessage = "New password is required.")]
		[MinLength(8, ErrorMessage = "New Password must be at least 8 characters long.")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
			ErrorMessage = "New password must contain at least one uppercase, one lowercase, one digit, and one special character.")]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "New Password confirmation is required")]
		[Compare("NewPassword", ErrorMessage = "New Password and new confirmation password do not match")]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}
}
