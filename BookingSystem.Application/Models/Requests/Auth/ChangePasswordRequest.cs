using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class ChangePasswordRequest
	{
		[Required(ErrorMessage = "Current password is required")]
		public string CurrentPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "New password is required")]
		[StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password confirmation is required")]
		[Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match")]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}
}
