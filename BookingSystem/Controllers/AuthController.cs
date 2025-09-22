using BookingSystem.Application.Contracts;
using BookingSystem.Application.Models.Requests.Auth;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

		public AuthController(
			IAuthService authService,
			IJwtService jwtService,
			UserManager<User> userManager)
		{
			_authService = authService;
			_jwtService = jwtService;
			_userManager = userManager;
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
				return Ok(new ApiResponse<AuthResponse>
				{
					Success = true,
					Message = "2FA code sent to your email",
					Data = new AuthResponse
					{
						RequiresTwoFactor = true,
						User = await _authService.GetUserInfoAsync(user)
					}
				});
			}

			var roles = await _userManager.GetRolesAsync(user);
			var accessToken = _jwtService.GenerateAccessToken(user, roles);

			var authResponse = new AuthResponse
			{
				AccessToken = accessToken,
				RefreshToken = user.RefreshToken!,
				AccessTokenExpires = DateTime.UtcNow.AddMinutes(15),
				RefreshTokenExpires = user.RefreshTokenExpiryTime.Value,
				User = await _authService.GetUserInfoAsync(user)
			};

			return Ok(new ApiResponse<AuthResponse>
			{
				Success = true,
				Message = "Login successful",
				Data = authResponse
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

				var authResponse = new AuthResponse
				{
					AccessToken = accessToken,
					RefreshToken = user.RefreshToken!,
					AccessTokenExpires = DateTime.UtcNow.AddMinutes(15),
					User = await _authService.GetUserInfoAsync(user)
				};

				return Ok(new ApiResponse<AuthResponse>
				{
					Success = true,
					Message = "2FA verification successful",
					Data = authResponse
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
		public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(RefreshTokenRequest request)
		{
			try
			{
				var user = await _authService.RefreshTokenAsync(request.RefreshToken);
				var roles = await _userManager.GetRolesAsync(user);
				var accessToken = _jwtService.GenerateAccessToken(user, roles);

				var authResponse = new AuthResponse
				{
					AccessToken = accessToken,
					RefreshToken = user.RefreshToken!,
					AccessTokenExpires = DateTime.UtcNow.AddMinutes(15),
					User = await _authService.GetUserInfoAsync(user)
				};

				return Ok(new ApiResponse<AuthResponse>
				{
					Success = true,
					Message = "Token refreshed successfully",
					Data = authResponse
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
	}
}
