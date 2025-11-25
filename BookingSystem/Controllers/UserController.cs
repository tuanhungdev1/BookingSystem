using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.UserDTO;
using BookingSystem.Application.DTOs.Users;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Macs;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly UserManager<User> _userManager;
		private readonly IGenericExportService _exportService;

		public UserController(
			IUserService userService,
			 IGenericExportService exportService,
			UserManager<User> userManager)
		{
			_userService = userService;
			_userManager = userManager;
			_exportService = exportService;
		}

		#region User Retrieval

		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<UserDto>>> GetUserProfile(int id)
		{
			try
			{
				var user = await _userService.GetUserAsync(id);
				return Ok(new ApiResponse<UserDto>
				{
					Success = true,
					Message = "User profile retrieved successfully",
					Data = user
				});
			}
			catch (Exception ex)
			{
				return NotFound(new ApiResponse<UserDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpGet("by-email/{email}")]
		public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByEmail(string email)
		{
			try
			{
				var user = await _userService.GetUserByEmailAsync(email);
				return Ok(new ApiResponse<UserDto>
				{
					Success = true,
					Message = "User retrieved successfully",
					Data = user
				});
			}
			catch (Exception ex)
			{
				return NotFound(new ApiResponse<UserDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
		{
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out var userId))
			{
				return Unauthorized(new ApiResponse<UserDto>
				{
					Success = false,
					Message = "User not authenticated"
				});
			}

			try
			{
				var user = await _userService.GetUserAsync(userId);
				return Ok(new ApiResponse<UserDto>
				{
					Success = true,
					Message = "Current user information retrieved successfully",
					Data = user
				});
			}
			catch (Exception ex)
			{
				return NotFound(new ApiResponse<UserDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetUsers([FromQuery] UserFilter userFilter)
		{
			try
			{
				var users = await _userService.GetUsersAsync(userFilter);
				return Ok(new ApiResponse<PagedResult<UserDto>>
				{
					Success = true,
					Message = "Users retrieved successfully",
					Data = users
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<PagedResult<UserDto>>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		#endregion

		#region User Management

		[HttpPost]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
		{
			try
			{
				var user = await _userService.CreateUserAsync(createUserDto);
				return CreatedAtAction(nameof(GetUserProfile), new { id = user.Id },
					new ApiResponse<UserDto>
					{
						Success = true,
						Message = "User created successfully",
						Data = user
					});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<UserDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpPut("{id:int}")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateUser(int id, UpdateUserDto updateUserDto)
		{
			if (!IsAuthorizedForUser(id))
				return Forbid();

			try
			{
				await _userService.UpdateUserAsync(id, updateUserDto);
				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "User updated successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpPut("user-profile")]
		[Authorize]
		public async Task<ActionResult<ApiResponse<object>>> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUser)
		{
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			try
			{
				var userData = await _userService.UpdateUserProfileAsync(int.Parse(currentUserId), updateUser);
				return Ok(new ApiResponse<UserProfileDto>
				{
					Success = true,
					Message = "User updated successfully",
					Data = userData
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpDelete("{id:int}")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
		{
			try
			{
				await _userService.DeleteUserAsync(id);
				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "User deleted successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		#endregion

		#region User Status & Lock Management

		[HttpPut("{id:int}/status")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> ChangeUserStatus(int id, [FromBody] bool isActive)
		{
			try
			{
				var success = await _userService.ChangeUserStatusAsync(id, isActive);
				if (!success)
					return NotFound(new ApiResponse<object>
					{
						Success = false,
						Message = "User not found"
					});

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = $"User status changed to {(isActive ? "active" : "inactive")}"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpPut("{id:int}/unlock")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UnlockUser(int id)
		{
			try
			{
				var success = await _userService.UnlockUserAsync(id);
				if (!success)
					return NotFound(new ApiResponse<object>
					{
						Success = false,
						Message = "User not found"
					});

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "User unlocked successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpPut("{id:int}/lock")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> LockUser(int id)
		{
			try
			{
				var success = await _userService.LockUserAsync(id);
				if (!success)
					return NotFound(new ApiResponse<object>
					{
						Success = false,
						Message = "User not found"
					});

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "User locked successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		#endregion

		#region Role Management

		[HttpPost("{id:int}/roles")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> AssignRoles(int id, [FromBody] IEnumerable<string> roles)
		{
			try
			{
				var success = await _userService.AssignRolesAsync(id, roles);
				if (!success)
					return BadRequest(new ApiResponse<object>
					{
						Success = false,
						Message = "Failed to assign roles"
					});

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "Roles assigned successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpDelete("{id:int}/roles")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> RemoveRoles(int id, [FromBody] IEnumerable<string> roles)
		{
			try
			{
				var success = await _userService.RemoveRolesAsync(id, roles);
				if (!success)
					return BadRequest(new ApiResponse<object>
					{
						Success = false,
						Message = "Failed to remove roles"
					});

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "Roles removed successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpGet("{id:int}/roles")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetUserRoles(int id)
		{
			try
			{
				var roles = await _userService.GetUserRolesAsync(id);
				return Ok(new ApiResponse<IEnumerable<string>>
				{
					Success = true,
					Message = "User roles retrieved successfully",
					Data = roles
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<IEnumerable<string>>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		#endregion

		#region Password Management

		[HttpPut("{id:int}/password")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<object>>> UpdateUserPassword(int id, [FromBody] string newPassword)
		{
			try
			{
				var success = await _userService.UpdateUserPasswordAsync(id, newPassword);
				if (!success)
					return BadRequest(new ApiResponse<object>
					{
						Success = false,
						Message = "Failed to update password"
					});

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "Password updated successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		#endregion

		#region Avatar Management

		[HttpPost("{id:int}/avatar")]
		public async Task<ActionResult<ApiResponse<UserAvatarDto>>> UploadUserAvatar(int id, [FromForm] UploadAvatarRequest uploadAvatarDto)
		{

			if (uploadAvatarDto?.Image == null || uploadAvatarDto.Image.Length == 0)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "No file uploaded"
				});
			}



			if (!IsAuthorizedForUser(id))
				return Forbid();

			try
			{
				await _userService.UploadUserAvatarAsync(id, uploadAvatarDto.Image);
				var avatarDto = await _userService.GetUserAvatarAsync(id);

				return Ok(new ApiResponse<UserAvatarDto>
				{
					Success = true,
					Message = "Avatar uploaded successfully",
					Data = avatarDto
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<UserAvatarDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpDelete("{id:int}/avatar")]
		public async Task<ActionResult<ApiResponse<object>>> DeleteUserAvatar(int id)
		{
			if (!IsAuthorizedForUser(id))
				return Forbid();

			try
			{
				var success = await _userService.DeleteUserAvatarAsync(id);
				if (!success)
					return NotFound(new ApiResponse<object>
					{
						Success = false,
						Message = "User or avatar not found"
					});

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "Avatar deleted successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpGet("{id:int}/avatar")]
		[AllowAnonymous]
		public async Task<ActionResult<ApiResponse<UserAvatarDto>>> GetUserAvatar(int id)
		{
			try
			{
				var avatar = await _userService.GetUserAvatarAsync(id);
				if (avatar == null)
					return NotFound(new ApiResponse<UserAvatarDto>
					{
						Success = false,
						Message = "Avatar not found"
					});

				return Ok(new ApiResponse<UserAvatarDto>
				{
					Success = true,
					Message = "Avatar retrieved successfully",
					Data = avatar
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<UserAvatarDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		#endregion

		#region Statistics

		[HttpGet("stats/total-count")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<int>>> GetTotalUsersCount()
		{
			try
			{
				var count = await _userService.GetTotalUsersCountAsync();
				return Ok(new ApiResponse<int>
				{
					Success = true,
					Message = "Total users count retrieved successfully",
					Data = count
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<int>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		[HttpGet("stats/active-count")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<int>>> GetActiveUsersCount()
		{
			try
			{
				var count = await _userService.GetActiveUsersCountAsync();
				return Ok(new ApiResponse<int>
				{
					Success = true,
					Message = "Active users count retrieved successfully",
					Data = count
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<int>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		// ============================== EXPORT USERS (Admin only) ==============================

		[HttpGet("export/excel")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ExportUsersToExcel([FromQuery] UserFilter filter)
		{
			try
			{
				var result = await _userService.GetUsersAsync(filter);

				var exportData = result.Items.Select(u => new UserExportDto
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					FirstName = u.FirstName,
					LastName = u.LastName,
					FullName = u.FullName,
					PhoneNumber = u.PhoneNumber ?? "N/A",
					Gender = u.Gender?.ToString(),
					DateOfBirth = u.DateOfBirth,
					Address = u.Address,
					City = u.City,
					Country = u.Country,
					PostalCode = u.PostalCode,
					Roles = u.Roles != null ? string.Join(", ", u.Roles) : "N/A",
					IsActive = u.IsActive,
					IsLocked = u.IsLocked,
					IsEmailConfirmed = u.IsEmailConfirmed,
					CreatedAt = u.CreatedAt,
					LastLoginAt = u.LastLoginAt
				});

				var fileBytes = await _exportService.ExportToExcelAsync(
					exportData,
					sheetName: "Danh sách người dùng",
					fileName: "users.xlsx"
				);

				return File(fileBytes,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					$"users_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Xuất Excel thất bại: {ex.Message}"
				});
			}
		}

		[HttpGet("export/pdf")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ExportUsersToPdf([FromQuery] UserFilter filter)
		{
			try
			{
				var result = await _userService.GetUsersAsync(filter);

				var exportData = result.Items.Select(u => new UserExportDto
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					FirstName = u.FirstName,
					LastName = u.LastName,
					FullName = u.FullName,
					PhoneNumber = u.PhoneNumber ?? "N/A",
					Gender = u.Gender?.ToString(),
					DateOfBirth = u.DateOfBirth,
					Address = u.Address,
					City = u.City,
					Country = u.Country,
					PostalCode = u.PostalCode,
					Roles = u.Roles != null ? string.Join(", ", u.Roles) : "N/A",
					IsActive = u.IsActive,
					IsLocked = u.IsLocked,
					IsEmailConfirmed = u.IsEmailConfirmed,
					CreatedAt = u.CreatedAt,
					LastLoginAt = u.LastLoginAt
				});

				var fileBytes = await _exportService.ExportToPdfAsync(
					exportData,
					title: "DANH SÁCH NGƯỜI DÙNG - BOOKING SYSTEM",
					fileName: "users.pdf"
				);

				return File(fileBytes, "application/pdf", $"users_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Xuất PDF thất bại: {ex.Message}"
				});
			}
		}

		[HttpGet("export/csv")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ExportUsersToCSV([FromQuery] UserFilter filter)
		{
			try
			{
				var result = await _userService.GetUsersAsync(filter);

				var exportData = result.Items.Select(u => new UserExportDto
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					FirstName = u.FirstName,
					LastName = u.LastName,
					FullName = u.FullName,
					PhoneNumber = u.PhoneNumber ?? "N/A",
					Gender = u.Gender?.ToString(),
					DateOfBirth = u.DateOfBirth,
					Address = u.Address,
					City = u.City,
					Country = u.Country,
					PostalCode = u.PostalCode,
					Roles = u.Roles != null ? string.Join(", ", u.Roles) : "N/A",
					IsActive = u.IsActive,
					IsLocked = u.IsLocked,
					IsEmailConfirmed = u.IsEmailConfirmed,
					CreatedAt = u.CreatedAt,
					LastLoginAt = u.LastLoginAt
				});

				var fileBytes = await _exportService.ExportToCSVAsync(
					exportData,
					fileName: "users.csv"
				);

				return File(fileBytes, "text/csv; charset=utf-8", $"users_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = $"Xuất CSV thất bại: {ex.Message}"
				});
			}
		}

		[HttpGet("stats/overview")]
		[Authorize(Policy = "Admin")]
		public async Task<ActionResult<ApiResponse<UserStatisticsDto>>> GetUserStatistics()
		{
			try
			{
				var stats = await _userService.GetUserStatisticsAsync();
				return Ok(new ApiResponse<UserStatisticsDto>
				{
					Success = true,
					Message = "User statistics retrieved successfully",
					Data = stats
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ApiResponse<UserStatisticsDto>
				{
					Success = false,
					Message = ex.Message
				});
			}
		}

		#endregion

		#region Helper Methods

		private bool IsAuthorizedForUser(int userId)
		{
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var isAdmin = User.IsInRole("Admin");

			return isAdmin || (currentUserId != null && currentUserId == userId.ToString());
		}

		#endregion

		
	}
}