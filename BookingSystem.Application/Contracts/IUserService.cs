using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Application.Contracts
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
        Task<UserProfileDto?> GetUserByEmailAsync(string email);
		Task<UserProfileDto> CreateUserAsync(CreateUserDto createUserDto);
		Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto request);
        Task<bool> DeleteUserAsync(Guid userId);
		Task<PagedResult<UserProfileDto>> GetUsersAsync(UserFilter userFilter);
		Task<bool> ChangeUserStatusAsync(Guid userId, bool isActive);
		Task<bool> AssignRolesAsync(Guid userId, IEnumerable<string> roles);
		Task<bool> RemoveRolesAsync(Guid userId, IEnumerable<string> roles);
		Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
		Task<bool> UpdateUserPasswordAsync(Guid userId, string newPassword);
		Task<bool> UpdateUserAvatarAsync(Guid userId, string avatarUrl);
		Task<int> GetTotalUsersCountAsync();
		Task<bool> UnlockUserAsync(Guid userId);
		Task<bool> LockUserAsync(Guid userId);
		Task<bool> ResetUserPasswordAsync(Guid userId, string resetToken, string newPassword);
	}
}
