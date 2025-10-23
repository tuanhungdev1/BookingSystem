using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using BookingSystem.Application.DTOs.ImageDTO;
using BookingSystem.Application.Models.Constants;
using Microsoft.Identity.Client;
using BookingSystem.Application.DTOs.Users;
using BookingSystem.Application.Models.Responses;

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
		private readonly ICloudinaryService _cloudinaryService;

		public UserService(
			IUserRepository userRepository,
			ILogger<UserService> logger,
			IMapper mapper,
			UserManager<User> userManager,
			RoleManager<IdentityRole<int>> roleManager,
			ICloudinaryService cloudinaryService,
			IUnitOfWork unitOfWork)
		{
			_userRepository = userRepository;
			_logger = logger;
			_mapper = mapper;
			_userManager = userManager;
			_roleManager = roleManager;
			_unitOfWork = unitOfWork;
			_cloudinaryService = cloudinaryService;
		}

		public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto userProfile)
		{
			_logger.LogInformation("Starting profile update for User ID {UserId}.", userId);

			var user = await _userRepository.GetByIdAsync(userId);

			if (user == null)
			{
				_logger.LogWarning("User with ID {UserId} not found. Cannot update profile.", userId);
				throw new BadRequestException($"Không tìm thấy người dùng với ID {userId}.");
			}

			_logger.LogInformation("Mapping updated profile data for User ID {UserId}.", userId);
			_mapper.Map(userProfile, user);

			_userRepository.Update(user);
			await _userRepository.SaveChangesAsync();

			_logger.LogInformation("Successfully updated profile for User ID {UserId}.", userId);

			return _mapper.Map<UserProfileDto>(user);
		}


		#region User CRUD Operations

		public async Task<UserDto?> GetUserAsync(int userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				return user == null
					? throw new NotFoundException($"User not found with ID: {userId}")
					: await MapToUserDtoAsync(user);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user profile for ID: {UserId}", userId);
				throw;
			}
		}

		public async Task<UserDto?> GetUserByEmailAsync(string email)
		{
			try
			{
				var user = await _userManager.FindByEmailAsync(email);
				return user == null
					? throw new BadRequestException($"User not found with email: {email}")
					: await MapToUserDtoAsync(user);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user by email: {Email}", email);
				throw;
			}
		}

		public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
		{
			var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
			if (existingUser != null)
			{
				_logger.LogError("User creation failed: Email {Email} already exists", createUserDto.Email);
				throw new BadRequestException($"Email '{createUserDto.Email}' already exists");
			}

			try
			{
				var user = _mapper.Map<User>(createUserDto);
				user.IsEmailConfirmed = true;
				user.EmailConfirmed = true;
				user.UserName = user.Email;

				var result = await _userManager.CreateAsync(user, createUserDto.Password);
				if (!result.Succeeded)
				{
					var errors = string.Join(", ", result.Errors.Select(e => e.Description));
					_logger.LogError("User creation failed: {Errors}", errors);
					throw new BadRequestException($"User creation failed: {errors}");
				}

				// Assign roles if provided
				if (createUserDto.Roles?.Any() == true)
				{
					await AssignRolesAsync(user.Id, createUserDto.Roles);
				}

				_logger.LogInformation("User created successfully: {Email}", user.Email);

				return await MapToUserDtoAsync(user);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating user with email: {Email}", createUserDto.Email);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto request)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString())
				?? throw new NotFoundException($"User not found for update with ID: {userId}");

			try
			{
				// THÊM: Kiểm tra email không bị trùng (nếu có thay đổi)
				if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
				{
					var existingUser = await _userManager.FindByEmailAsync(request.Email);
					if (existingUser != null)
					{
						throw new BadRequestException($"Email '{request.Email}' is already in use");
					}
				}

				// Map DTO sang User entity (chỉ update các field có giá trị)
				if (!string.IsNullOrEmpty(request.FirstName))
					user.FirstName = request.FirstName;

				if (!string.IsNullOrEmpty(request.LastName))
					user.LastName = request.LastName;

				if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
					user.Email = request.Email;

				if (request.DateOfBirth.HasValue)
					user.DateOfBirth = request.DateOfBirth;

				if (request.Gender.HasValue)
					user.Gender = request.Gender;

				if (!string.IsNullOrEmpty(request.Address))
					user.Address = request.Address;

				if (!string.IsNullOrEmpty(request.City))
					user.City = request.City;

				if (!string.IsNullOrEmpty(request.Country))
					user.Country = request.Country;

				if (!string.IsNullOrEmpty(request.PostalCode))
					user.PostalCode = request.PostalCode;

				if (!string.IsNullOrEmpty(request.PhoneNumber))
					user.PhoneNumber = request.PhoneNumber;

				if (request.IsActive.HasValue)
					user.IsActive = request.IsActive.Value;

				if (request.IsLocked.HasValue)
				{
					// THÊM: Xử lý Lock/Unlock logic
					if (request.IsLocked.Value)
					{
						await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
					}
					else
					{
						await _userManager.SetLockoutEndDateAsync(user, null);
					}
				}

				user.UpdatedAt = DateTime.UtcNow;

				var result = await _userManager.UpdateAsync(user);
				if (!result.Succeeded)
				{
					var errors = string.Join(", ", result.Errors.Select(e => e.Description));
					_logger.LogError("User update failed: {Errors}", errors);
					throw new BadRequestException($"User update failed: {errors}");
				}

				// THÊM: Update roles nếu được cung cấp
				if (request.Roles?.Any() == true)
				{
					await AssignRolesAsync(user.Id, request.Roles);
				}

				_logger.LogInformation("User updated successfully: {UserId}", userId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating user with ID: {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> DeleteUserAsync(int userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString())
				?? throw new NotFoundException($"User not found for deletion with ID: {userId}");

			try
			{

				user.IsDeleted = true;
				user.UpdatedAt = DateTime.UtcNow;
				await _userManager.UpdateAsync(user);

				_logger.LogInformation("User deleted successfully: {UserId}", userId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting user with ID: {UserId}", userId);
				throw;
			}
		}

		public async Task<PagedResult<UserDto>> GetUsersAsync(UserFilter userFilter)
		{
			try
			{
				var pagedUsers = await _userRepository.GetPagedAsync(userFilter);
				var userDtos = new List<UserDto>();

				foreach (var user in pagedUsers.Items)
				{
					userDtos.Add(await MapToUserDtoAsync(user));
				}

				return new PagedResult<UserDto>
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

		#endregion

		#region User Status Management

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
					_logger.LogInformation("User status changed: {UserId}, Active: {IsActive}", userId, isActive);
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

				user.IsLocked = false;

				var result = await _userManager.SetLockoutEndDateAsync(user, null);
				if (result.Succeeded)
				{
					user.UpdatedAt = DateTime.UtcNow;
					await _userManager.UpdateAsync(user);
					_logger.LogInformation("User unlocked: {UserId}", userId);
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

				user.IsLocked = true;

				var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
				if (result.Succeeded)
				{
					user.UpdatedAt = DateTime.UtcNow;
					await _userManager.UpdateAsync(user);
					_logger.LogInformation("User locked: {UserId}", userId);
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

		#endregion

		#region Role Management

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

				// Validate all roles exist
				foreach (var role in roles)
				{
					if (!await _roleManager.RoleExistsAsync(role))
					{
						_logger.LogError("Role {Role} does not exist", role);
						throw new BadRequestException($"Role '{role}' does not exist");
					}
				}

				// Remove existing roles first
				var currentRoles = await _userManager.GetRolesAsync(user);
				if (currentRoles.Any())
				{
					await _userManager.RemoveFromRolesAsync(user, currentRoles);
				}

				var result = await _userManager.AddToRolesAsync(user, roles);
				if (result.Succeeded)
				{
					_logger.LogInformation("Roles assigned to user: {UserId}, Roles: {Roles}", userId, string.Join(", ", roles));
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
					_logger.LogInformation("Roles removed from user: {UserId}, Roles: {Roles}", userId, string.Join(", ", roles));
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

		#endregion

		#region Password Management

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
					await RevokeRefreshTokensAsync(user);
					_logger.LogInformation("Password updated: {UserId}", userId);
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
					await RevokeRefreshTokensAsync(user);
					_logger.LogInformation("Password reset: {UserId}", userId);
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

		#endregion

		#region Avatar Management

		public async Task<bool> UploadUserAvatarAsync(int userId, IFormFile image)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString())
				?? throw new NotFoundException($"User not found with ID: {userId}");

			// Validate file
			const long maxFileSize = 5 * 1024 * 1024; // 5MB
			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

			if (image.Length > maxFileSize)
				throw new BadRequestException("File size must not exceed 5MB");

			var extension = Path.GetExtension(image.FileName).ToLower();
			if (!allowedExtensions.Contains(extension))
				throw new BadRequestException("Only jpg, jpeg, png, gif formats are allowed");

			var publicId = string.Empty;

			try
			{
				// THAY ĐỔI: Không dùng transaction - xóa avatar cũ trước, upload mới, rồi update DB

				// 1. Delete old avatar if exists
				if (!string.IsNullOrEmpty(user.Avatar))
				{
					var oldPublicId = _cloudinaryService.GetPublicIdFromUrl(user.Avatar);
					if (!string.IsNullOrEmpty(oldPublicId))
					{
						await _cloudinaryService.DeleteImageAsync(oldPublicId);
						_logger.LogInformation("Old avatar deleted: PublicId: {PublicId}", oldPublicId);
					}
				}

				// 2. Upload new avatar
				var imageParam = new ImageUploadDto
				{
					File = image,
					Folder = $"{FolderImages.Users}/{user.Id}"
				};

				var uploadResult = await _cloudinaryService.UploadImageAsync(imageParam);
				if (!uploadResult.Success)
					throw new BadRequestException($"Failed to upload image: {uploadResult.ErrorMessage}");

				publicId = uploadResult.Data.PublicId;

				// 3. Update user avatar URL
				user.Avatar = uploadResult.Data.Url;
				user.UpdatedAt = DateTime.UtcNow;

				var result = await _userManager.UpdateAsync(user);
				if (!result.Succeeded)
				{
					var errors = string.Join(", ", result.Errors.Select(e => e.Description));
					throw new BadRequestException($"Failed to update user avatar: {errors}");
				}

				_logger.LogInformation("Avatar uploaded successfully: {UserId}", userId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error uploading avatar for user: {UserId}", userId);

				// Cleanup: Delete uploaded image on failure
				if (!string.IsNullOrEmpty(publicId))
				{
					try
					{
						await _cloudinaryService.DeleteImageAsync(publicId);
					}
					catch (Exception cleanupEx)
					{
						_logger.LogWarning(cleanupEx, "Failed to cleanup image: {PublicId}", publicId);
					}
				}

				throw;
			}
		}

		public async Task<bool> DeleteUserAvatarAsync(int userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString())
				?? throw new NotFoundException($"User not found with ID: {userId}");

			if (string.IsNullOrEmpty(user.Avatar))
			{
				_logger.LogWarning("User has no avatar to delete: {UserId}", userId);
				return false;
			}

			try
			{
				// THAY ĐỔI: Không dùng transaction

				// 1. Delete from Cloudinary
				var publicId = _cloudinaryService.GetPublicIdFromUrl(user.Avatar);
				if (!string.IsNullOrEmpty(publicId))
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete avatar from Cloudinary: {PublicId}", publicId);
						// Tiếp tục - không throw exception
					}
				}

				// 2. Clear avatar URL from DB
				user.Avatar = null;
				user.UpdatedAt = DateTime.UtcNow;

				var result = await _userManager.UpdateAsync(user);
				if (!result.Succeeded)
				{
					var errors = string.Join(", ", result.Errors.Select(e => e.Description));
					throw new BadRequestException($"Failed to delete user avatar: {errors}");
				}

				_logger.LogInformation("Avatar deleted successfully: {UserId}", userId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting avatar for user: {UserId}", userId);
				throw;
			}
		}

		public async Task<UserAvatarDto?> GetUserAvatarAsync(int userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null || string.IsNullOrEmpty(user.Avatar))
				{
					return null;
				}

				var publicId = _cloudinaryService.GetPublicIdFromUrl(user.Avatar);
				return new UserAvatarDto
				{
					UserId = user.Id,
					AvatarUrl = user.Avatar,
					PublicId = publicId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting avatar for user: {UserId}", userId);
				throw;
			}
		}

		#endregion

		#region Statistics

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

		public async Task<int> GetActiveUsersCountAsync()
		{
			try
			{
				return await _userManager.Users
					.Where(u => u.IsActive && !u.IsDeleted)
					.CountAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting active users count");
				throw;
			}
		}

		public async Task<UserStatisticsDto> GetUserStatisticsAsync()
		{
			try
			{
				var now = DateTime.UtcNow;
				var currentMonth = new DateTime(now.Year, now.Month, 1);
				var lastMonth = currentMonth.AddMonths(-1);
				var nextMonth = currentMonth.AddMonths(1);

				var newUsersThisMonth = await _userManager.Users
					.Where(u => u.CreatedAt >= currentMonth && u.CreatedAt < nextMonth && !u.IsDeleted)
					.CountAsync();

				var newUsersLastMonth = await _userManager.Users
					.Where(u => u.CreatedAt >= lastMonth && u.CreatedAt < currentMonth && !u.IsDeleted)
					.CountAsync();

				var percentageChange = newUsersLastMonth > 0
					? ((decimal)(newUsersThisMonth - newUsersLastMonth) / newUsersLastMonth) * 100
					: (newUsersThisMonth > 0 ? 100 : 0);

				var totalUsers = await GetTotalUsersCountAsync();
				var totalActiveUsers = await GetActiveUsersCountAsync();
				var deletedUsers = totalUsers - (await _userManager.Users.Where(u => !u.IsDeleted).CountAsync());

				return new UserStatisticsDto
				{
					TotalActiveUsers = totalActiveUsers,
					NewUsersThisMonth = newUsersThisMonth,
					NewUsersLastMonth = newUsersLastMonth,
					PercentageChange = Math.Round(percentageChange, 2),
					TrendDirection = percentageChange >= 0 ? "up" : "down",
					TotalUsers = totalUsers,
					DeletedUsers = deletedUsers
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user statistics");
				throw;
			}
		}

		#endregion

		#region Helper Methods

		private async Task<UserDto> MapToUserDtoAsync(User user)
		{
			var userDto = _mapper.Map<UserDto>(user);
			var roles = await _userManager.GetRolesAsync(user);
			userDto.Roles = roles;
			return userDto;
		}

		private async Task RevokeRefreshTokensAsync(User user)
		{
			user.RefreshToken = null;
			user.RefreshTokenExpiryTime = null;
			user.UpdatedAt = DateTime.UtcNow;
			await _userManager.UpdateAsync(user);
		}

		#endregion
	}
}