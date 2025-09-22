namespace BookingSystem.Application.Models.Responses
{
	public class AuthResponse : BaseResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
		public DateTime AccessTokenExpires { get; set; }
		public DateTime RefreshTokenExpires { get; set; }
		public UserProfileDto User { get; set; } = new();
		public bool RequiresTwoFactor { get; set; }
	}
}
