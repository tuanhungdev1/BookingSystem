using BookingSystem.Application.Contracts;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using BookingSystem.Application.Models.Requests.Auth;
using BookingSystem.Domain.Enums;
using BookingSystem.Application.Models.Common;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using BookingSystem.Domain.Base;
using BookingSystem.Application.Models.Responses;
using AutoMapper;
using BookingSystem.Domain.Repositories;

namespace BookingSystem.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IEmailService _emailService;
		private readonly AppSettings _appSettings;
		private readonly ILogger<AuthService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IUserRepository _userRepository;
		

		public AuthService(
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			IEmailService emailService,
			IOptions<AppSettings> appSettings,
			ILogger<AuthService> logger,
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IUserRepository userRepository
			)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailService = emailService;
			_appSettings = appSettings.Value;
			_logger = logger;
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_userRepository = userRepository;
		}

		public async Task<User> AdminLoginAsync(LoginRequest loginRequest)
		{
			var existingUser = await _userManager.FindByEmailAsync(loginRequest.Email);

			// Kiểm tra email có tồn tại hay không
			if (existingUser == null)
				throw new BadRequestException("Invalid email or password");

			// Kiểm tra xác thực email
			if (!existingUser.IsEmailConfirmed)
				throw new BadRequestException("Please confirm your email before logging in");

			// Kiểm tra user có phải là Admin không
			var roles = await _userManager.GetRolesAsync(existingUser);
			if (!roles.Contains(SystemRoles.Admin.ToString()))
				throw new BadRequestException("You do not have admin privileges");

			// Kiểm tra mật khẩu
			var result = await _signInManager.CheckPasswordSignInAsync(existingUser, loginRequest.Password, lockoutOnFailure: false);
			if (!result.Succeeded)
				throw new BadRequestException("Invalid email or password");

			// Cập nhật thời gian đăng nhập cuối
			existingUser.LastLoginAt = DateTime.UtcNow;
			// Tạo refresh token
			existingUser.RefreshToken = GenerateSecureToken();
			existingUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
			await _userManager.UpdateAsync(existingUser);

			return existingUser;
		}

		public async Task<User?> RegisterAsync(RegisterRequest request)
		{
			var existingUser = await _userManager.FindByEmailAsync(request.Email);
			if (existingUser != null)
			{
				_logger.LogError("Registration failed: Email {Email} already exists", request.Email);
				throw new BadRequestException("Email already exists");
			}


			var user = new User
			{
				UserName = request.Email,
				Email = request.Email,
				FirstName = request.FirstName,
				LastName = request.LastName,
				Gender = request.Gender,
				DateOfBirth = request.DateOfBirth,
				Address = request.Address,
				City = request.City,
				Country = request.Country,
				PhoneNumber = request.PhoneNumber,
				CreatedAt = DateTime.UtcNow,
				IsEmailConfirmed = false,
				EmailConfirmed = false,
				EmailConfirmationToken = GenerateSecureToken(),
				EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24)
			};

			var result = await _userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				throw new BadRequestException($"Registration failed: {errors}");
			}

			await _userManager.AddToRoleAsync(user, SystemRoles.User.ToString());

			// Gửi email xác nhận
			var confirmationLink = $"{_appSettings.ClientUrl}/confirm-email?email={user.Email}&token={user.EmailConfirmationToken}";
			await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);

			_logger.LogInformation("User registered successfully: {Email}", user.Email);
			return user;
		}

		public async Task<(User? user, bool requiresTwoFactor)> LoginAsync(LoginRequest request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null)
				throw new BadRequestException("Invalid email or password");

			if (!user.IsEmailConfirmed)
				throw new BadRequestException("Please confirm your email before logging in");

			var result = await _signInManager.PasswordSignInAsync(
						user,
						request.Password,
						request.RememberMe,
						lockoutOnFailure: false);
			if (!result.Succeeded)
				throw new BadRequestException("Invalid email or password");

			// Kiểm tra 2FA
			if (user.TwoFactorEnabled)
			{
				await GenerateAndSend2FACodeAsync(user.Email!);
				return (user, true);
			}

			// Tạo refresh token
			user.RefreshToken = GenerateSecureToken();
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
			await _userManager.UpdateAsync(user);

			return (user, false);
		}

		public async Task<User?> RefreshTokenAsync(string refreshToken)
		{
			var user = await _unitOfWork.UserRepository.GetByRefreshTokenAsync(refreshToken);

			if (user == null)
				throw new SecurityTokenException("Invalid or expired refresh token");

			// Tạo refresh token mới
			user.RefreshToken = GenerateSecureToken();
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
			await _userManager.UpdateAsync(user);

			return user;
		}

		public async Task<bool> LogoutAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return false;

			user.RefreshToken = null;
			user.RefreshTokenExpiryTime = null;
			await _userManager.UpdateAsync(user);

			return true;
		}

		public async Task<bool> ConfirmEmailAsync(string email, string token)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				throw new NotFoundException("User not found.");


			if (user.EmailConfirmed || user.IsEmailConfirmed)
				throw new BadRequestException("Email has already been verified. No further verification is needed.");


			if (user.EmailConfirmationToken != token ||
				user.EmailConfirmationTokenExpiry == null ||
				user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
				throw new BadRequestException("Invalid or expired verification link.");

			
			user.IsEmailConfirmed = true;
			user.EmailConfirmed = true; 
			user.EmailConfirmationToken = null;
			user.EmailConfirmationTokenExpiry = null;


			_userRepository.Update(user);
			await _userRepository.SaveChangesAsync();
			// Gửi email chào mừng
			await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName ?? "User");
			return true;
		}

		public async Task<bool> ResendEmailConfirmationAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null || user.IsEmailConfirmed)
				return false;

			user.EmailConfirmationToken = GenerateSecureToken();
			user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
			await _userManager.UpdateAsync(user);

			var confirmationLink = $"{_appSettings.ClientUrl}/confirm-email?email={user.Email}&token={user.EmailConfirmationToken}";
			return await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);
		}

		public async Task<bool> ForgotPasswordAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null || !user.IsEmailConfirmed)
				return false; // Không tiết lộ thông tin user có tồn tại hay không

			user.PasswordResetToken = GenerateSecureToken();
			user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
			await _userManager.UpdateAsync(user);

			var resetLink = $"{_appSettings.ClientUrl}/reset-password?email={user.Email}&token={user.PasswordResetToken}";
			return await _emailService.SendPasswordResetAsync(user.Email!, resetLink);
		}

		public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null || user.PasswordResetToken != request.Token ||
				user.PasswordResetTokenExpiry < DateTime.UtcNow)
				return false;

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

			if (result.Succeeded)
			{
				user.PasswordResetToken = null;
				user.PasswordResetTokenExpiry = null;
				user.RefreshToken = null; // Revoke all sessions
				user.RefreshTokenExpiryTime = null;
				await _userManager.UpdateAsync(user);
				return true;
			}

			return false;
		}

		public async Task<bool> GenerateAndSend2FACodeAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return false;

			var code = GenerateNumericCode(6);
			user.TwoFactorCode = code;
			user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5);
			user.TwoFactorCodeUsed = false;
			await _userManager.UpdateAsync(user);

			return await _emailService.Send2FACodeAsync(user.Email!, code);
		}

		public async Task<User?> Verify2FACodeAsync(string email, string code)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null || user.TwoFactorCode != code ||
				user.TwoFactorCodeExpiry < DateTime.UtcNow || user.TwoFactorCodeUsed)
				return null;

			user.TwoFactorCodeUsed = true;
			user.RefreshToken = GenerateSecureToken();
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
			await _userManager.UpdateAsync(user);

			return user;
		}

		public async Task<bool> Enable2FAAsync(string userId, bool enable)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return false;

			user.TwoFactorEnabled = enable;
			if (!enable)
			{
				// Clear any existing 2FA codes
				user.TwoFactorCode = null;
				user.TwoFactorCodeExpiry = null;
				user.TwoFactorCodeUsed = false;
			}

			var result = await _userManager.UpdateAsync(user);
			return result.Succeeded;
		}

		public async Task<(User user, bool isNewUser)> ExternalLoginAsync(
	string email,
	string externalId,
	AuthProvider provider,
	string firstName,
	string lastName,
	string? avatar = null)
		{
			// 1. Kiểm tra email đã tồn tại
			var existingUser = await _userManager.FindByEmailAsync(email);

			if (existingUser != null)
			{
				// Kiểm tra nếu email được đăng ký với provider khác
				if (existingUser.AuthProvider.HasValue &&
					existingUser.AuthProvider != provider)
				{
					_logger.LogWarning(
						"External login failed: Email {Email} already registered with {Provider}",
						email, existingUser.AuthProvider);

					throw new BadRequestException(
						$"This email is already registered with {existingUser.AuthProvider}. " +
						$"Please login using {existingUser.AuthProvider}.");
				}

				// Kiểm tra nếu email được đăng ký bằng password (Local)
				if (!existingUser.AuthProvider.HasValue ||
					existingUser.AuthProvider == AuthProvider.Local)
				{
					_logger.LogWarning(
						"External login failed: Email {Email} already registered with password",
						email);

					throw new BadRequestException(
						"This email is already registered with a password. " +
						"Please login using your email and password, or use the 'Forgot Password' option.");
				}

				// Cùng provider -> cập nhật thông tin và login
				existingUser.ExternalId = externalId;
				existingUser.LastLoginAt = DateTime.UtcNow;
				existingUser.RefreshToken = GenerateSecureToken();
				existingUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

				// Cập nhật avatar nếu có
				if (!string.IsNullOrEmpty(avatar) && string.IsNullOrEmpty(existingUser.Avatar))
				{
					existingUser.Avatar = avatar;
				}

				// Cập nhật tên nếu chưa có
				if (string.IsNullOrEmpty(existingUser.FirstName))
				{
					existingUser.FirstName = firstName;
				}
				if (string.IsNullOrEmpty(existingUser.LastName))
				{
					existingUser.LastName = lastName;
				}

				existingUser.UpdatedAt = DateTime.UtcNow;
				await _userManager.UpdateAsync(existingUser);

				_logger.LogInformation(
					"User logged in successfully via {Provider}: {Email}",
					provider, email);

				return (existingUser, false);
			}

			// 2. Tạo user mới
			var newUser = new User
			{
				UserName = email,
				Email = email,
				FirstName = firstName,
				LastName = lastName,
				AuthProvider = provider,
				ExternalId = externalId,
				ExternalEmail = email,
				Avatar = avatar,
				IsEmailConfirmed = true, // OAuth emails are pre-verified
				EmailConfirmed = true,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				RefreshToken = GenerateSecureToken(),
				RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
			};

			// Tạo user không cần password
			var result = await _userManager.CreateAsync(newUser);

			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				_logger.LogError(
					"External login failed for {Email}: {Errors}",
					email, errors);

				throw new BadRequestException($"External login failed: {errors}");
			}

			// Gán role User
			await _userManager.AddToRoleAsync(newUser, SystemRoles.User.ToString());

			// Gửi welcome email
			await _emailService.SendWelcomeEmailAsync(newUser.Email!, newUser.FirstName);

			_logger.LogInformation(
				"New user created via {Provider}: {Email}",
				provider, email);

			return (newUser, true);
		}

		public async Task<UserProfileDto> GetUserInfoAsync(User user)
		{
			var roles = await _userManager.GetRolesAsync(user);
			var dto = _mapper.Map<UserProfileDto>(user);
			dto.Roles = roles;
			return dto;
		}

		private string GenerateSecureToken()
		{
			var randomNumber = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);
			return Convert.ToBase64String(randomNumber).Replace("+", "-").Replace("/", "_").Replace("=", "");
		}

		private string GenerateNumericCode(int length)
		{
			var random = new Random();
			var code = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				code.Append(random.Next(0, 10));
			}
			return code.ToString();
		}
	}
}
