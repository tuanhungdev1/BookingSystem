using BookingSystem.Application.Contracts;
using BookingSystem.Application.Models.Common;
using BookingSystem.Application.Models.Requests.Auth;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Entities;
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

		public AuthController(
			IAuthService authService,
			IOptions<JwtSettings> jwtSettings,
			IJwtService jwtService,
			IWebHostEnvironment environment,
			UserManager<User> userManager)
		{
			_authService = authService;
			_jwtService = jwtService;
			_userManager = userManager;
			_jwtSettings = jwtSettings.Value;
			_environment = environment;
		}

		[HttpPost("login/admin")]
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> AdminLogin(LoginRequest request)
		{
			var user = await _authService.AdminLoginAsync(request);

			var roles = await _userManager.GetRolesAsync(user);
			var accessToken = _jwtService.GenerateAccessToken(user, roles);

			SetTokenCookie("accessToken", accessToken, int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()));
			SetTokenCookie("refreshToken", user.RefreshToken, int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60);

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

			SetTokenCookie("accessToken", accessToken, int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()));
			SetTokenCookie("refreshToken", user.RefreshToken, int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60);

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


				SetTokenCookie("accessToken", accessToken, int.Parse(_jwtSettings.AccessTokenExpirationMinutes.ToString()));
				SetTokenCookie("refreshToken", user.RefreshToken, int.Parse(_jwtSettings.RefreshTokenExpiryDays.ToString()) * 24 * 60);


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

		private void SetTokenCookie(string name, string value, int expirationMinutes)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Lax, 
				Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
			};
			Response.Cookies.Append(name, value, cookieOptions);
		}
	}
}
