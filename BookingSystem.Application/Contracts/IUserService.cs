using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.DTOs.Users;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.Contracts
{
	public interface IUserService
	{
		// User CRUD
		Task<UserDto?> GetUserAsync(int userId);
		Task<UserDto?> GetUserByEmailAsync(string email);
		Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
		Task<bool> UpdateUserAsync(int userId, UpdateUserDto request);
		Task<bool> DeleteUserAsync(int userId);
		Task<PagedResult<UserDto>> GetUsersAsync(UserFilter userFilter);

		// User Status & Authentication
		Task<bool> ChangeUserStatusAsync(int userId, bool isActive);
		Task<bool> UnlockUserAsync(int userId);
		Task<bool> LockUserAsync(int userId);

		// Role Management
		Task<bool> AssignRolesAsync(int userId, IEnumerable<string> roles);
		Task<bool> RemoveRolesAsync(int userId, IEnumerable<string> roles);
		Task<IEnumerable<string>> GetUserRolesAsync(int userId);

		// Password Management
		Task<bool> UpdateUserPasswordAsync(int userId, string newPassword);
		Task<bool> ResetUserPasswordAsync(int userId, string resetToken, string newPassword);

		// Avatar Management
		Task<bool> UploadUserAvatarAsync(int userId, IFormFile image);
		Task<bool> DeleteUserAvatarAsync(int userId);
		Task<UserAvatarDto?> GetUserAvatarAsync(int userId);

		// Statistics
		Task<int> GetTotalUsersCountAsync();
		Task<int> GetActiveUsersCountAsync();
		Task<UserStatisticsDto> GetUserStatisticsAsync();
		Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto userProfile);
	}
}
