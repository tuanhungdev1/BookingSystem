using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class Verify2FARequest
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Verification code is required.")]
		public string Code { get; set; } = string.Empty;
	}
}
