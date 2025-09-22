using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class ResetPasswordAdminRequest
	{
		[Required(ErrorMessage = "New password is required.")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be between 6 and 100 characters.")]
		public string NewPassword { get; set; } = string.Empty;
	}
}
