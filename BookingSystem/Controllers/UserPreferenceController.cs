using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.UserPreferenceDTO;
using BookingSystem.Application.Models.Constants;
using BookingSystem.Application.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UserPreferenceController : ControllerBase
	{
		private readonly IUserPreferenceService _userPreferenceService;

		public UserPreferenceController(IUserPreferenceService userPreferenceService)
		{
			_userPreferenceService = userPreferenceService;
		}

		/// <summary>
		/// Get all preferences for current user
		/// </summary>
		[HttpGet]
		public async Task<ActionResult<ApiResponse<IEnumerable<UserPreferenceDto>>>> GetMyPreferences()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<IEnumerable<UserPreferenceDto>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var preferences = await _userPreferenceService.GetAllUserPreferencesAsync(userId);

			return Ok(new ApiResponse<IEnumerable<UserPreferenceDto>>
			{
				Success = true,
				Message = "Preferences retrieved successfully",
				Data = preferences
			});
		}

		/// <summary>
		/// Get specific preference by key
		/// </summary>
		[HttpGet("{preferenceKey}")]
		public async Task<ActionResult<ApiResponse<UserPreferenceDto>>> GetPreference(string preferenceKey)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<UserPreferenceDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var preference = await _userPreferenceService.GetPreferenceAsync(userId, preferenceKey);
			if (preference == null)
			{
				return NotFound(new ApiResponse<UserPreferenceDto>
				{
					Success = false,
					Message = $"Preference with key '{preferenceKey}' not found"
				});
			}

			return Ok(new ApiResponse<UserPreferenceDto>
			{
				Success = true,
				Message = "Preference retrieved successfully",
				Data = preference
			});
		}

		/// <summary>
		/// Get preference value by key (returns just the value)
		/// </summary>
		[HttpGet("{preferenceKey}/value")]
		public async Task<ActionResult<ApiResponse<string>>> GetPreferenceValue(
			string preferenceKey,
			[FromQuery] string? defaultValue = null)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var value = await _userPreferenceService.GetPreferenceValueAsync(userId, preferenceKey, defaultValue);

			return Ok(new ApiResponse<string>
			{
				Success = true,
				Message = "Preference value retrieved successfully",
				Data = value
			});
		}

		/// <summary>
		/// Get multiple preferences by keys
		/// </summary>
		[HttpPost("by-keys")]
		public async Task<ActionResult<ApiResponse<Dictionary<string, string>>>> GetPreferencesByKeys(
			[FromBody] List<string> keys)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<Dictionary<string, string>>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var preferences = await _userPreferenceService.GetPreferencesByKeysAsync(userId, keys);

			return Ok(new ApiResponse<Dictionary<string, string>>
			{
				Success = true,
				Message = "Preferences retrieved successfully",
				Data = preferences
			});
		}

		/// <summary>
		/// Create a new preference
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<ApiResponse<UserPreferenceDto>>> CreatePreference(
			[FromBody] CreateUserPreferenceDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<UserPreferenceDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var preference = await _userPreferenceService.CreatePreferenceAsync(userId, request);

			return CreatedAtAction(
				nameof(GetPreference),
				new { preferenceKey = preference.PreferenceKey },
				new ApiResponse<UserPreferenceDto>
				{
					Success = true,
					Message = "Preference created successfully",
					Data = preference
				}
			);
		}

		/// <summary>
		/// Update an existing preference
		/// </summary>
		[HttpPut("{preferenceKey}")]
		public async Task<ActionResult<ApiResponse<UserPreferenceDto>>> UpdatePreference(
			string preferenceKey,
			[FromBody] UpdateUserPreferenceDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<UserPreferenceDto>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var preference = await _userPreferenceService.UpdatePreferenceAsync(userId, preferenceKey, request);
			if (preference == null)
			{
				return NotFound(new ApiResponse<UserPreferenceDto>
				{
					Success = false,
					Message = $"Preference with key '{preferenceKey}' not found"
				});
			}

			return Ok(new ApiResponse<UserPreferenceDto>
			{
				Success = true,
				Message = "Preference updated successfully",
				Data = preference
			});
		}

		/// <summary>
		/// Bulk set/update multiple preferences
		/// </summary>
		[HttpPost("bulk")]
		public async Task<ActionResult<ApiResponse<int>>> BulkSetPreferences(
			[FromBody] BulkUserPreferenceDto request)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<int>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var updatedCount = await _userPreferenceService.BulkSetPreferencesAsync(userId, request);

			return Ok(new ApiResponse<int>
			{
				Success = true,
				Message = $"{updatedCount} preferences updated successfully",
				Data = updatedCount
			});
		}

		/// <summary>
		/// Delete a specific preference
		/// </summary>
		[HttpDelete("{preferenceKey}")]
		public async Task<ActionResult<ApiResponse<object>>> DeletePreference(string preferenceKey)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var success = await _userPreferenceService.DeletePreferenceAsync(userId, preferenceKey);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to delete preference"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Preference deleted successfully"
			});
		}

		/// <summary>
		/// Delete all preferences for current user
		/// </summary>
		[HttpDelete]
		public async Task<ActionResult<ApiResponse<object>>> DeleteAllPreferences()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var success = await _userPreferenceService.DeleteAllPreferencesAsync(userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to delete preferences"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "All preferences deleted successfully"
			});
		}

		/// <summary>
		/// Reset preferences to default values
		/// </summary>
		[HttpPost("reset-to-defaults")]
		public async Task<ActionResult<ApiResponse<object>>> ResetToDefaults()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var success = await _userPreferenceService.ResetToDefaultsAsync(userId);
			if (!success)
			{
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to reset preferences"
				});
			}

			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Preferences reset to defaults successfully"
			});
		}

		/// <summary>
		/// Get preference as boolean value
		/// </summary>
		[HttpGet("{preferenceKey}/bool")]
		public async Task<ActionResult<ApiResponse<bool>>> GetBoolPreference(
			string preferenceKey,
			[FromQuery] bool defaultValue = false)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<bool>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var value = await _userPreferenceService.GetBoolPreferenceAsync(userId, preferenceKey, defaultValue);

			return Ok(new ApiResponse<bool>
			{
				Success = true,
				Message = "Preference value retrieved successfully",
				Data = value
			});
		}

		/// <summary>
		/// Get preference as integer value
		/// </summary>
		[HttpGet("{preferenceKey}/int")]
		public async Task<ActionResult<ApiResponse<int>>> GetIntPreference(
			string preferenceKey,
			[FromQuery] int defaultValue = 0)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<int>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var value = await _userPreferenceService.GetIntPreferenceAsync(userId, preferenceKey, defaultValue);

			return Ok(new ApiResponse<int>
			{
				Success = true,
				Message = "Preference value retrieved successfully",
				Data = value
			});
		}

		/// <summary>
		/// Get preference as decimal value
		/// </summary>
		[HttpGet("{preferenceKey}/decimal")]
		public async Task<ActionResult<ApiResponse<decimal>>> GetDecimalPreference(
			string preferenceKey,
			[FromQuery] decimal defaultValue = 0m)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized(new ApiResponse<decimal>
				{
					Success = false,
					Message = "Invalid user authentication"
				});
			}

			var value = await _userPreferenceService.GetDecimalPreferenceAsync(userId, preferenceKey, defaultValue);

			return Ok(new ApiResponse<decimal>
			{
				Success = true,
				Message = "Preference value retrieved successfully",
				Data = value
			});
		}

		/// <summary>
		/// Get all available preference keys (for documentation/helper)
		/// </summary>
		[HttpGet("available-keys")]
		[AllowAnonymous]
		public ActionResult<ApiResponse<Dictionary<string, string>>> GetAvailablePreferenceKeys()
		{
			var availableKeys = new Dictionary<string, string>
			{
				// Display preferences
				{ PreferenceKeys.Language, "User interface language (en, vi, etc.)" },
				{ PreferenceKeys.Theme, "UI theme (light, dark)" },
				{ PreferenceKeys.Currency, "Preferred currency (VND, USD, etc.)" },
				{ PreferenceKeys.DateFormat, "Date display format" },
				{ PreferenceKeys.TimeFormat, "Time display format (12h, 24h)" },

				// Notification preferences
				{ PreferenceKeys.EmailNotifications, "Enable email notifications (true/false)" },
				{ PreferenceKeys.PushNotifications, "Enable push notifications (true/false)" },
				{ PreferenceKeys.SmsNotifications, "Enable SMS notifications (true/false)" },
				{ PreferenceKeys.MarketingEmails, "Receive marketing emails (true/false)" },

				// Booking preferences
				{ PreferenceKeys.DefaultGuests, "Default number of guests" },
				{ PreferenceKeys.DefaultCheckInTime, "Preferred check-in time" },
				{ PreferenceKeys.DefaultCheckOutTime, "Preferred check-out time" },
				{ PreferenceKeys.PreferredPaymentMethod, "Preferred payment method" },

				// Privacy preferences
				{ PreferenceKeys.ProfileVisibility, "Profile visibility (public, private, friends)" },
				{ PreferenceKeys.ShowEmail, "Show email on profile (true/false)" },
				{ PreferenceKeys.ShowPhone, "Show phone on profile (true/false)" },

				// Search preferences
				{ PreferenceKeys.DefaultSearchRadius, "Default search radius in km" },
				{ PreferenceKeys.DefaultPriceRange, "Default price range filter" },
				{ PreferenceKeys.PreferredAmenities, "Preferred amenities (comma-separated)" },

				// Host preferences
				{ PreferenceKeys.AutoAcceptBookings, "Auto-accept bookings (true/false)" },
				{ PreferenceKeys.MinimumAdvanceBooking, "Minimum advance booking hours" },
				{ PreferenceKeys.MaximumAdvanceBooking, "Maximum advance booking days" }
			};

			return Ok(new ApiResponse<Dictionary<string, string>>
			{
				Success = true,
				Message = "Available preference keys retrieved successfully",
				Data = availableKeys
			});
		}
	}
}