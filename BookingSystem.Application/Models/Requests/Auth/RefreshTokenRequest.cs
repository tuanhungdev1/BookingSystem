using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Models.Requests.Auth
{
	public class RefreshTokenRequest
	{
		[Required(ErrorMessage = "Refresh token is required")]
		public string RefreshToken { get; set; } = string.Empty;
	}
}
