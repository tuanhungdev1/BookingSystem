namespace BookingSystem.Application.Models.Responses
{
	public class TokenResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
		public string TokenType { get; set; } = "Bearer";
		public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
		public DateTime ExpiresAt { get; set; }
	}
}
