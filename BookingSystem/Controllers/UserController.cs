using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize] // Require authentication for all endpoints
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly UserManager<User> _userManager;

		public UserController(
			IUserService userService,
			UserManager<User> userManager)
		{
			_userService = userService;
			_userManager = userManager;
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserProfile(int id)
		{
			var user = await _userService.GetUserProfileAsync(id);
			if (user == null)
			{
				return NotFound(new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = "User not found"
				});
			}

			return Ok(new ApiResponse<UserProfileDto>
			{
				Success = true,
				Message = "User profile retrieved successfully",
				Data = user
			});
		}

		[HttpGet("by-email/{email}")]
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserByEmail(string email)
		{
			var user = await _userService.GetUserByEmailAsync(email);
			if (user == null)
			{
				return NotFound(new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = "User not found"
				});
			}

			return Ok(new ApiResponse<UserProfileDto>
			{
				Success = true,
				Message = "User retrieved successfully",
				Data = user
			});
		}

		[HttpPost]
		[Authorize(Roles = "SuperAdmin,Admin")] // Only admins can create users
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> CreateUser(CreateUserDto createUserDto)
		{
			var user = await _userService.CreateUserAsync(createUserDto);
			return Ok(new ApiResponse<UserProfileDto>
			{
				Success = true,
				Message = "User created successfully",
				Data = user
			});
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateUser(int id, UpdateUserDto updateUserDto)
		{
			// Users can only update their own profile unless they're admin
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");

			if (!isAdmin && currentUserId != id.ToString())
			{
				return Forbid();
			}

			var success = await _userService.UpdateUserAsync(id, updateUserDto);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "User not found or update failed"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "User updated successfully"
			});
		}

		[HttpDelete("{id:int}")]
		[Authorize(Roles = "SuperAdmin")] // Only super admin can delete users
		public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
		{
			var success = await _userService.DeleteUserAsync(id);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "User not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "User deleted successfully"
			});
		}

		[HttpGet]
		[Authorize(Roles = "SuperAdmin,Admin")] // Only admins can list all users
		public async Task<ActionResult<ApiResponse<PagedResult<UserProfileDto>>>> GetUsers([FromQuery] UserFilter userFilter)
		{
			var users = await _userService.GetUsersAsync(userFilter);
			return Ok(new ApiResponse<PagedResult<UserProfileDto>>
			{
				Success = true,
				Message = "Users retrieved successfully",
				Data = users
			});
		}

		[HttpPut("{id:int}/status")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ChangeUserStatus(int id, [FromBody] bool isActive)
		{
			var success = await _userService.ChangeUserStatusAsync(id, isActive);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "User not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = $"User status changed to {(isActive ? "active" : "inactive")} successfully"
			});
		}

		[HttpPost("{id:int}/roles")]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<ActionResult<ApiResponse<object>>> AssignRoles(int id, [FromBody] IEnumerable<string> roles)
		{
			var success = await _userService.AssignRolesAsync(id, roles);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to assign roles"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Roles assigned successfully"
			});
		}

		[HttpDelete("{id:int}/roles")]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<ActionResult<ApiResponse<object>>> RemoveRoles(int id, [FromBody] IEnumerable<string> roles)
		{
			var success = await _userService.RemoveRolesAsync(id, roles);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to remove roles"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Roles removed successfully"
			});
		}

		[HttpGet("{id:int}/roles")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetUserRoles(int id)
		{
			var roles = await _userService.GetUserRolesAsync(id);
			return Ok(new ApiResponse<IEnumerable<string>>
			{
				Success = true,
				Message = "User roles retrieved successfully",
				Data = roles
			});
		}

		[HttpPut("{id:int}/password")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateUserPassword(int id, [FromBody] string newPassword)
		{
			var success = await _userService.UpdateUserPasswordAsync(id, newPassword);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to update password"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Password updated successfully"
			});
		}

		[HttpPut("{id:int}/avatar")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateUserAvatar(int id, [FromBody] string avatarUrl)
		{
			// Users can only update their own avatar unless they're admin
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");

			if (!isAdmin && currentUserId != id.ToString())
			{
				return Forbid();
			}

			var success = await _userService.UpdateUserAvatarAsync(id, avatarUrl);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "User not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Avatar updated successfully"
			});
		}

		[HttpGet("count")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<ActionResult<ApiResponse<int>>> GetTotalUsersCount()
		{
			var count = await _userService.GetTotalUsersCountAsync();
			return Ok(new ApiResponse<int>
			{
				Success = true,
				Message = "Total users count retrieved successfully",
				Data = count
			});
		}

		[HttpPut("{id:int}/unlock")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UnlockUser(int id)
		{
			var success = await _userService.UnlockUserAsync(id);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "User not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "User unlocked successfully"
			});
		}

		[HttpPut("{id:int}/lock")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<ActionResult<ApiResponse<object>>> LockUser(int id)
		{
			var success = await _userService.LockUserAsync(id);
			if (!success)
			{
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "User not found"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "User locked successfully"
			});
		}

		//[HttpPost("{id:int}/reset-password")]
		//[Authorize(Roles = "SuperAdmin,Admin")]
		//public async Task<ActionResult<ApiResponse<object>>> ResetUserPassword(int id, [FromBody] ResetUserPasswordRequest request)
		//{
		//	try
		//	{
		//		var success = await _userService.ResetUserPasswordAsync(id, request.ResetToken, request.NewPassword);
		//		if (!success)
		//		{
		//			return BadRequest(new ApiResponse<object>
		//			{
		//				Success = false,
		//				Message = "Failed to reset password"
		//			});
		//		}

		//		return Ok(new ApiResponse<object>
		//		{
		//			Success = true,
		//			Message = "Password reset successfully"
		//		});
		//	}
		//	catch (Exception ex)
		//	{
		//		return StatusCode(500, new ApiResponse<object>
		//		{
		//			Success = false,
		//			Message = "An error occurred while resetting password"
		//		});
		//	}
		//}

		[HttpGet("me")]
		[AllowAnonymous] // Override the controller-level authorization
		[Authorize] // But still require authentication
		public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetCurrentUser()
		{
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(currentUserId))
			{
				return Unauthorized(new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = "User not authenticated"
				});
			}

			var user = await _userService.GetUserProfileAsync(int.Parse(currentUserId));
			if (user == null)
			{
				return NotFound(new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = "User not found"
				});
			}

			return Ok(new ApiResponse<UserProfileDto>
			{
				Success = true,
				Message = "Current user information retrieved successfully",
				Data = user
			});
		}
	}
}
