using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Application.Contracts
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<UserProfileDto?> GetUserByEmailAsync(string email);
		Task<UserProfileDto> CreateUserAsync(CreateUserDto createUserDto);
		Task<bool> UpdateUserAsync(int userId, UpdateUserDto request);
        Task<bool> DeleteUserAsync(int userId);
		Task<PagedResult<UserProfileDto>> GetUsersAsync(UserFilter userFilter);
		Task<bool> ChangeUserStatusAsync(int userId, bool isActive);
		Task<bool> AssignRolesAsync(int userId, IEnumerable<string> roles);
		Task<bool> RemoveRolesAsync(int userId, IEnumerable<string> roles);
		Task<IEnumerable<string>> GetUserRolesAsync(int userId);
		Task<bool> UpdateUserPasswordAsync(int userId, string newPassword);
		Task<bool> UpdateUserAvatarAsync(int userId, string avatarUrl);
		Task<int> GetTotalUsersCountAsync();
		Task<bool> UnlockUserAsync(int userId);
		Task<bool> LockUserAsync(int userId);
		Task<bool> ResetUserPasswordAsync(int userId, string resetToken, string newPassword);
	}
}
