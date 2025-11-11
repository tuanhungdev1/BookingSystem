using BookingSystem.Application.Models.Requests.Auth;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.Contracts
{
	public interface IAuthService
	{
		Task<User> RegisterAsync(RegisterRequest request);
		Task<(User? user, bool requiresTwoFactor)> LoginAsync(LoginRequest request);
		Task<(User user, bool isNewUser)> ExternalLoginAsync(
																string email,
																string externalId,
																AuthProvider provider,
																string firstName,
																string lastName,
																string? avatar = null);
		Task<User> AdminLoginAsync(LoginRequest loginRequest);
		Task<User?> RefreshTokenAsync(string refreshToken);
		Task<bool> LogoutAsync(string userId);
		Task<bool> ConfirmEmailAsync(string email, string token);
		Task<bool> ResendEmailConfirmationAsync(string email);
		Task<bool> ForgotPasswordAsync(string email);
		Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
		Task<bool> GenerateAndSend2FACodeAsync(string email);
		Task<User?> Verify2FACodeAsync(string email, string code);
		Task<bool> Enable2FAAsync(string userId, bool enable);
		Task<UserProfileDto> GetUserInfoAsync(User user);
	}
}
