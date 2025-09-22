using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class ConfirmEmailRequest
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Confirmation token is required.")]
		public string Token { get; set; } = string.Empty;
	}
}
