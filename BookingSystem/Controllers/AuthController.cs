using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.Models.Common;
using BookingSystem.Application.Models.Requests.Auth;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Exceptions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly IJwtService _jwtService;
		private readonly UserManager<User> _userManager;
		private readonly JwtSettings _jwtSettings;
		private readonly IWebHostEnvironment _environment;
		private readonly FacebookAuthSettings _facebookSettings;
		private readonly GoogleAuthSettings _googleSettings;
		public AuthController(
			IAuthService authService,
			IOptions<JwtSettings> jwtSettings,
			IJwtService jwtService,
			IWebHostEnvironment environment,
			IOptions<FacebookAuthSettings> facebookSettings,
			IOptions<GoogleAuthSettings> googleSettings,
			UserManager<User> userManager)
		{
			_authService = authService;
			_jwtService = jwtService;
			_userManager = userManager;
			_jwtSettings = jwtSettings.Value;
			_environment = environment;
			_googleSettings = googleSettings.Value;
			_facebookSettings = facebookSettings.Value;
		}

		[HttpPost("login/admin")]
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> AdminLogin(LoginRequest request)
		{
			var user = await _authService.AdminLoginAsync(request);

			var roles = await _userManager.GetRolesAsync(user);
			var accessToken = _jwtService.GenerateAccessToken(user, roles);

			SetTokenCookie("accessToken", accessToken,
				int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()),
				request.RememberMe);
			SetTokenCookie("refreshToken", user.RefreshToken,
				int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60,
				request.RememberMe);

			return Ok(new ApiResponse<UserProfileDto>
			{
				Success = true,
				Message = "Login successful",
				Data = await _authService.GetUserInfoAsync(user)
			});
		}

		[HttpPost("register")]
		public async Task<ActionResult<ApiResponse<object>>> Register(RegisterRequest request)
		{
			var user = await _authService.RegisterAsync(request);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Registration successful. Please check your email to confirm your account.",
				Data = new { Email = user.Email }
			});
		}

		[HttpPost("login")]
		public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
		{
			var (user, requiresTwoFactor) = await _authService.LoginAsync(request);

			if (requiresTwoFactor)
			{
				return Ok(new ApiResponse<UserProfileDto>
				{
					Success = true,
					Message = "2FA code sent to your email",
					Data = await _authService.GetUserInfoAsync(user)
				});
			}

			var roles = await _userManager.GetRolesAsync(user);
			var accessToken = _jwtService.GenerateAccessToken(user, roles);

			SetTokenCookie("accessToken", accessToken,
				int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()),
				request.RememberMe);
			SetTokenCookie("refreshToken", user.RefreshToken,
				int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60,
				request.RememberMe);

			return Ok(new ApiResponse<UserProfileDto>
			{
				Success = true,
				Message = "Login successful",
				Data = await _authService.GetUserInfoAsync(user)
			});
		}

		[HttpPost("verify-2fa")]
		public async Task<ActionResult<ApiResponse<AuthResponse>>> Verify2FA(Verify2FARequest request)
		{
			try
			{
				var user = await _authService.Verify2FACodeAsync(request.Email, request.Code);
				if (user == null)
				{
					return BadRequest(new ApiResponse<AuthResponse>
					{
						Success = false,
						Message = "Invalid or expired 2FA code"
					});
				}

				var roles = await _userManager.GetRolesAsync(user);
				var accessToken = _jwtService.GenerateAccessToken(user, roles);


				SetTokenCookie("accessToken", accessToken,
					int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()));
				SetTokenCookie("refreshToken", user.RefreshToken,
					int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60);

				return Ok(new ApiResponse<UserProfileDto>
				{
					Success = true,
					Message = "2FA verification successful",
					Data = await _authService.GetUserInfoAsync(user)
				});
			}
			catch (Exception)
			{
				return StatusCode(500, new ApiResponse<AuthResponse>
				{
					Success = false,
					Message = "An error occurred during 2FA verification"
				});
			}
		}

		[HttpPost("refresh-token")]
		public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken()
		{
			try
			{

				var refreshToken = Request.Cookies["refreshToken"];

				if (string.IsNullOrEmpty(refreshToken))
				{
					return Unauthorized(new ApiResponse<UserProfileDto>
					{
						Success = false,
						Message = "Refresh token not found"
					});
				}

				var user = await _authService.RefreshTokenAsync(refreshToken);

				if (user == null)
				{
					return Unauthorized(new ApiResponse<UserProfileDto>
					{
						Success = false,
						Message = "Invalid or expired refresh token"
					});
				}

				var roles = await _userManager.GetRolesAsync(user);
				var accessToken = _jwtService.GenerateAccessToken(user, roles);

				SetTokenCookie("accessToken", accessToken, int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()));
				SetTokenCookie("refreshToken", user.RefreshToken, int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60);

				return Ok(new ApiResponse<UserProfileDto>
				{
					Success = true,
					Message = "Token refreshed successfully",
					Data = await _authService.GetUserInfoAsync(user)
				});

			}
			catch (SecurityTokenException ex)
			{
				return Unauthorized(new ApiResponse<AuthResponse>
				{
					Success = false,
					Message = ex.Message
				});
			}
			catch (Exception)
			{
				return StatusCode(500, new ApiResponse<AuthResponse>
				{
					Success = false,
					Message = "An error occurred during token refresh"
				});
			}
		}

		[HttpPost("logout")]
		[Authorize]
		public async Task<ActionResult<ApiResponse<object>>> Logout()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var success = await _authService.LogoutAsync(userId!);

			var refreshToken = Request.Cookies["refreshToken"];

			// Clear cookies
			Response.Cookies.Delete("accessToken");
			Response.Cookies.Delete("refreshToken");

			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "Logged out successfully" : "Logout failed"
			});
		}

		[HttpPost("confirm-email")]
		public async Task<ActionResult<ApiResponse<object>>> ConfirmEmail(ConfirmEmailRequest request)
		{
			var success = await _authService.ConfirmEmailAsync(request.Email, request.Token);

			if (success)
			{
				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "Email confirmed successfully"
				});
			}

			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Message = "Invalid or expired confirmation token"
			});
		}

		[HttpPost("resend-confirmation")]
		public async Task<ActionResult<ApiResponse<object>>> ResendConfirmation(ResendConfirmationRequest request)
		{
			var success = await _authService.ResendEmailConfirmationAsync(request.Email);

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = success ? "Confirmation email sent" : "Email already confirmed or not found"
			});
		}

		[HttpPost("forgot-password")]
		public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(ForgotPasswordRequest request)
		{
			await _authService.ForgotPasswordAsync(request.Email);

			// Luôn trả về success để không tiết lộ thông tin user
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "If your email exists, a password reset link has been sent"
			});
		}

		[HttpPost("reset-password")]
		public async Task<ActionResult<ApiResponse<object>>> ResetPassword(ResetPasswordRequest request)
		{
			var success = await _authService.ResetPasswordAsync(request);

			if (success)
			{
				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "Password reset successfully"
				});
			}

			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Message = "Invalid or expired reset token"
			});
		}

		[HttpPost("enable-2fa")]
		[Authorize]
		public async Task<ActionResult<ApiResponse<object>>> Enable2FA(Enable2FARequest request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var success = await _authService.Enable2FAAsync(userId!, request.Enable);

			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? $"2FA {(request.Enable ? "enabled" : "disabled")} successfully" : "Failed to update 2FA setting"
			});
		}

		[HttpPost("send-2fa-code")]
		public async Task<ActionResult<ApiResponse<object>>> Send2FACode([FromBody] string email)
		{
			var success = await _authService.GenerateAndSend2FACodeAsync(email);

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = success ? "2FA code sent" : "Failed to send 2FA code"
			});
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetCurrentUser()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userManager.FindByIdAsync(userId!);

			if (user == null)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "User not found"
				});
			}

			var userInfo = await _authService.GetUserInfoAsync(user);

			return Ok(new ApiResponse<UserProfileDto>
			{
				Success = true,
				Message = "User information retrieved successfully",
				Data = userInfo
			});
		}

		[HttpPost("external-login/google")]
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> GoogleLogin(
		[FromBody] ExternalLoginRequest request)
		{
			try
			{
				// TODO: Implement Google token verification
				// For production, verify the IdToken with Google API
				// Example:
				var payload = await VerifyGoogleTokenAsync(request.IdToken);
				if (payload == null || payload.Email != request.Email)
				{
					return BadRequest(new ApiResponse<UserProfileDto>
					{
						Success = false,
						Message = "Invalid Google token"
					});
				}

				var (user, isNewUser) = await _authService.ExternalLoginAsync(
					request.Email,
					request.ExternalId,
					AuthProvider.Google,
					request.FirstName,
					request.LastName,
					request.Avatar);

				var roles = await _userManager.GetRolesAsync(user);
				var accessToken = _jwtService.GenerateAccessToken(user, roles);

				// Set cookies với expire dài hạn cho external login
				SetTokenCookie("accessToken", accessToken,
					int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()),
					rememberMe: true);
				SetTokenCookie("refreshToken", user.RefreshToken!,
					int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60,
					rememberMe: true);

				var userInfo = await _authService.GetUserInfoAsync(user);

				return Ok(new ApiResponse<UserProfileDto>
				{
					Success = true,
					Message = isNewUser
						? "Account created successfully. Welcome!"
						: "Login successful",
					Data = userInfo
				});
			}
			catch (BadRequestException ex)
			{
				return BadRequest(new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = "An error occurred during Google login"
				});
			}
		}

		[HttpPost("external-login/facebook")]
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> FacebookLogin(
			[FromBody] ExternalLoginRequest request)
		{
			try
			{
				// TODO: Implement Facebook token verification
				// For production, verify the IdToken with Facebook Graph API
				// Example:
				var isValid = await VerifyFacebookTokenAsync(request.IdToken, request.ExternalId);
				if (!isValid)
				{
					return BadRequest(new ApiResponse<UserProfileDto>
					{
						Success = false,
						Message = "Invalid Facebook token"
					});
				}

				var (user, isNewUser) = await _authService.ExternalLoginAsync(
					request.Email,
					request.ExternalId,
					AuthProvider.Facebook,
					request.FirstName,
					request.LastName,
					request.Avatar);

				var roles = await _userManager.GetRolesAsync(user);
				var accessToken = _jwtService.GenerateAccessToken(user, roles);

				// Set cookies với expire dài hạn cho external login
				SetTokenCookie("accessToken", accessToken,
					int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()),
					rememberMe: true);
				SetTokenCookie("refreshToken", user.RefreshToken!,
					int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60,
					rememberMe: true);

				var userInfo = await _authService.GetUserInfoAsync(user);

				return Ok(new ApiResponse<UserProfileDto>
				{
					Success = true,
					Message = isNewUser
						? "Account created successfully. Welcome!"
						: "Login successful",
					Data = userInfo
				});
			}
			catch (BadRequestException ex)
			{
				return BadRequest(new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = "An error occurred during Facebook login"
				});
			}
		}

		// Helper method để verify Google token (Optional - for production)
		private async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken)
		{
			try
			{
				// Cần cài package: Google.Apis.Auth
				var settings = new GoogleJsonWebSignature.ValidationSettings
				{
					Audience = new[] { _googleSettings.ClientId } // Your Google Client ID
				};

				var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
				return payload;
			}
			catch
			{
				return null;
			}
		}

		// Helper method để verify Facebook token (Optional - for production)
		private async Task<bool> VerifyFacebookTokenAsync(string accessToken, string userId)
		{
			try
			{
				using var httpClient = new HttpClient();

				// Facebook App Token = APP_ID|APP_SECRET
				var appToken = $"{_facebookSettings.AppId}|{_facebookSettings.AppSecret}";

				var url =
					$"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appToken}";

				var response = await httpClient.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					// Parse JSON nếu cần kiểm tra userId
					return true;
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

		private void SetTokenCookie(string name, string value, int expirationMinutes, bool rememberMe = false)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = true, // Chỉ gửi qua HTTPS
				SameSite = SameSiteMode.Lax, // Bảo vệ CSRF
				Expires = rememberMe
					? DateTimeOffset.UtcNow.AddDays(7) // 7 ngày nếu remember me
					: (DateTimeOffset?)null // Session cookie nếu không remember
			};

			// Nếu không remember me nhưng vẫn muốn set expire time
			if (!rememberMe && expirationMinutes > 0)
			{
				cookieOptions.Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
			}

			Response.Cookies.Append(name, value, cookieOptions);
		}
	}
}
