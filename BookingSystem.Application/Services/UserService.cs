using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly ILogger<UserService> _logger;
		private readonly IMapper _mapper;
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole<int>> _roleManager;
		private readonly IUnitOfWork _unitOfWork;

		public UserService(
			IUserRepository userRepository,
			ILogger<UserService> logger,
			IMapper mapper,
			UserManager<User> userManager,
			RoleManager<IdentityRole<int>> roleManager,
			IUnitOfWork unitOfWork)
		{
			_userRepository = userRepository;
			_logger = logger;
			_mapper = mapper;
			_userManager = userManager;
			_roleManager = roleManager;
			_unitOfWork = unitOfWork;
		}

		public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found with ID: {UserId}", userId);
					return null;
				}

				var userDto = _mapper.Map<UserProfileDto>(user);
				var roles = await _userManager.GetRolesAsync(user);
				userDto.Roles = roles;

				return userDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user profile for ID: {UserId}", userId);
				throw;
			}
		}

		public async Task<UserProfileDto?> GetUserByEmailAsync(string email)
		{
			try
			{
				var user = await _userManager.FindByEmailAsync(email);
				if (user == null)
				{
					_logger.LogWarning("User not found with email: {Email}", email);
					return null;
				}

				var userDto = _mapper.Map<UserProfileDto>(user);
				var roles = await _userManager.GetRolesAsync(user);
				userDto.Roles = roles;

				return userDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user by email: {Email}", email);
				throw;
			}
		}

		public async Task<UserProfileDto> CreateUserAsync(CreateUserDto createUserDto)
		{
			try
			{
				var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
				if (existingUser != null)
				{
					_logger.LogError("User creation failed: Email {Email} already exists", createUserDto.Email);
					throw new BadRequestException("Email already exists");
				}

				var user = new User
				{
					UserName = createUserDto.Email,
					Email = createUserDto.Email,
					FirstName = createUserDto.FirstName,
					LastName = createUserDto.LastName,
					Gender = createUserDto.Gender,
					DateOfBirth = createUserDto.DateOfBirth,
					Address = createUserDto.Address,
					City = createUserDto.City,
					Country = createUserDto.Country,
					PhoneNumber = createUserDto.PhoneNumber,
					IsActive = createUserDto.IsActive,
					CreatedAt = DateTime.UtcNow,
					EmailConfirmed = createUserDto.IsEmailConfirmed ?? false,
					IsEmailConfirmed = createUserDto.IsEmailConfirmed ?? false
				};

				var result = await _userManager.CreateAsync(user, createUserDto.Password);
				if (!result.Succeeded)
				{
					var errors = string.Join(", ", result.Errors.Select(e => e.Description));
					_logger.LogError("User creation failed: {Errors}", errors);
					throw new BadRequestException($"User creation failed: {errors}");
				}

				// Assign roles if provided
				if (createUserDto.Roles != null && createUserDto.Roles.Any())
				{
					await AssignRolesAsync(user.Id, createUserDto.Roles);
				}

				_logger.LogInformation("User created successfully: {Email}", user.Email);

				var userDto = _mapper.Map<UserProfileDto>(user);
				var roles = await _userManager.GetRolesAsync(user);
				userDto.Roles = roles;

				return userDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating user with email: {Email}", createUserDto.Email);
				throw;
			}
		}

		public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto request)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for update with ID: {UserId}", userId);
					return false;
				}

				// Update user properties
				user.FirstName = request.FirstName ?? user.FirstName;
				user.LastName = request.LastName ?? user.LastName;
				user.Gender = request.Gender ?? user.Gender;
				user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;
				user.Address = request.Address ?? user.Address;
				user.City = request.City ?? user.City;
				user.Country = request.Country ?? user.Country;
				user.PhoneNumber = request.PhoneNumber ?? request.PhoneNumber;
				user.UpdatedAt = DateTime.UtcNow;

				var result = await _userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					_logger.LogInformation("User updated successfully: {UserId}", userId);
					return true;
				}

				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				_logger.LogError("User update failed: {Errors}", errors);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating user with ID: {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> DeleteUserAsync(int userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for deletion with ID: {UserId}", userId);
					return false;
				}

				var result = await _userManager.DeleteAsync(user);
				if (result.Succeeded)
				{
					_logger.LogInformation("User deleted successfully: {UserId}", userId);
					return true;
				}

				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				_logger.LogError("User deletion failed: {Errors}", errors);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting user with ID: {UserId}", userId);
				throw;
			}
		}

		public async Task<PagedResult<UserProfileDto>> GetUsersAsync(UserFilter userFilter)
		{
			try
			{
				var pagedUsers = await _userRepository.GetPagedAsync(userFilter);
				var userDtos = new List<UserProfileDto>();

				foreach (var user in pagedUsers.Items)
				{
					var userDto = _mapper.Map<UserProfileDto>(user);
					var roles = await _userManager.GetRolesAsync(user);
					userDto.Roles = roles;
					userDtos.Add(userDto);
				}

				return new PagedResult<UserProfileDto>
				{
					Items = userDtos,
					TotalCount = pagedUsers.TotalCount,
					PageNumber = pagedUsers.PageNumber,
					PageSize = pagedUsers.PageSize,
					TotalPages = pagedUsers.TotalPages
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting paged users");
				throw;
			}
		}

		public async Task<bool> ChangeUserStatusAsync(int userId, bool isActive)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for status change with ID: {UserId}", userId);
					return false;
				}

				user.IsActive = isActive;
				user.UpdatedAt = DateTime.UtcNow;

				var result = await _userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					_logger.LogInformation("User status changed successfully: {UserId}, Active: {IsActive}", userId, isActive);
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error changing user status for ID: {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> AssignRolesAsync(int userId, IEnumerable<string> roles)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for role assignment with ID: {UserId}", userId);
					return false;
				}

				// Validate that all roles exist
				foreach (var role in roles)
				{
					if (!await _roleManager.RoleExistsAsync(role))
					{
						_logger.LogError("Role {Role} does not exist", role);
						throw new BadRequestException($"Role '{role}' does not exist");
					}
				}

				var result = await _userManager.AddToRolesAsync(user, roles);
				if (result.Succeeded)
				{
					_logger.LogInformation("Roles assigned successfully to user: {UserId}, Roles: {Roles}", userId, string.Join(", ", roles));
					return true;
				}

				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				_logger.LogError("Role assignment failed: {Errors}", errors);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error assigning roles to user: {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> RemoveRolesAsync(int userId, IEnumerable<string> roles)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for role removal with ID: {UserId}", userId);
					return false;
				}

				var result = await _userManager.RemoveFromRolesAsync(user, roles);
				if (result.Succeeded)
				{
					_logger.LogInformation("Roles removed successfully from user: {UserId}, Roles: {Roles}", userId, string.Join(", ", roles));
					return true;
				}

				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				_logger.LogError("Role removal failed: {Errors}", errors);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error removing roles from user: {UserId}", userId);
				throw;
			}
		}

		public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for getting roles with ID: {UserId}", userId);
					return Enumerable.Empty<string>();
				}

				return await _userManager.GetRolesAsync(user);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting roles for user: {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> UpdateUserPasswordAsync(int userId, string newPassword)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for password update with ID: {UserId}", userId);
					return false;
				}

				var token = await _userManager.GeneratePasswordResetTokenAsync(user);
				var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

				if (result.Succeeded)
				{
					// Revoke all refresh tokens to force re-authentication
					user.RefreshToken = null;
					user.RefreshTokenExpiryTime = null;
					user.UpdatedAt = DateTime.UtcNow;
					await _userManager.UpdateAsync(user);

					_logger.LogInformation("Password updated successfully for user: {UserId}", userId);
					return true;
				}

				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				_logger.LogError("Password update failed: {Errors}", errors);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating password for user: {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> UpdateUserAvatarAsync(int userId, string avatarUrl)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for avatar update with ID: {UserId}", userId);
					return false;
				}

				user.Avatar = avatarUrl;
				user.UpdatedAt = DateTime.UtcNow;

				var result = await _userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					_logger.LogInformation("Avatar updated successfully for user: {UserId}", userId);
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating avatar for user: {UserId}", userId);
				throw;
			}
		}

		public async Task<int> GetTotalUsersCountAsync()
		{
			try
			{
				return await _userManager.Users.CountAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting total users count");
				throw;
			}
		}

		public async Task<bool> UnlockUserAsync(int userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for unlock with ID: {UserId}", userId);
					return false;
				}

				var result = await _userManager.SetLockoutEndDateAsync(user, null);
				if (result.Succeeded)
				{
					user.UpdatedAt = DateTime.UtcNow;
					await _userManager.UpdateAsync(user);
					_logger.LogInformation("User unlocked successfully: {UserId}", userId);
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error unlocking user: {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> LockUserAsync(int userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for lock with ID: {UserId}", userId);
					return false;
				}

				// Lock user indefinitely
				var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
				if (result.Succeeded)
				{
					user.UpdatedAt = DateTime.UtcNow;
					await _userManager.UpdateAsync(user);
					_logger.LogInformation("User locked successfully: {UserId}", userId);
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error locking user: {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> ResetUserPasswordAsync(int userId, string resetToken, string newPassword)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null)
				{
					_logger.LogWarning("User not found for password reset with ID: {UserId}", userId);
					return false;
				}

				var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
				if (result.Succeeded)
				{
					// Revoke all refresh tokens
					user.RefreshToken = null;
					user.RefreshTokenExpiryTime = null;
					user.UpdatedAt = DateTime.UtcNow;
					await _userManager.UpdateAsync(user);

					_logger.LogInformation("Password reset successfully for user: {UserId}", userId);
					return true;
				}

				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				_logger.LogError("Password reset failed: {Errors}", errors);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
				throw;
			}
		}
	}
}
