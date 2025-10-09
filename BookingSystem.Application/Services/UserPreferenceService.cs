using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.UserPreferenceDTO;
using BookingSystem.Application.Models.Constants;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services
{
	public class UserPreferenceService : IUserPreferenceService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IUserPreferenceRepository _userPreferenceRepository;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<UserPreferenceService> _logger;

		public UserPreferenceService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IUserPreferenceRepository userPreferenceRepository,
			UserManager<User> userManager,
			ILogger<UserPreferenceService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_userPreferenceRepository = userPreferenceRepository;
			_userManager = userManager;
			_logger = logger;
		}

		#region CRUD Operations

		public async Task<UserPreferenceDto> CreatePreferenceAsync(int userId, CreateUserPreferenceDto request)
		{
			_logger.LogInformation("Creating preference {PreferenceKey} for user {UserId}.",
				request.PreferenceKey, userId);

			// Validate user
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			// Check if preference already exists
			var exists = await _userPreferenceRepository.PreferenceExistsAsync(userId, request.PreferenceKey);
			if (exists)
			{
				throw new BadRequestException($"Preference with key '{request.PreferenceKey}' already exists for this user.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var preference = new UserPreference
				{
					UserId = userId,
					PreferenceKey = request.PreferenceKey,
					PreferenceValue = request.PreferenceValue,
					DataType = request.DataType ?? "string",
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _userPreferenceRepository.AddAsync(preference);
				await _userPreferenceRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Preference {PreferenceKey} created successfully for user {UserId}.",
					request.PreferenceKey, userId);

				return _mapper.Map<UserPreferenceDto>(preference);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating preference.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<UserPreferenceDto?> UpdatePreferenceAsync(
			int userId,
			string preferenceKey,
			UpdateUserPreferenceDto request)
		{
			_logger.LogInformation("Updating preference {PreferenceKey} for user {UserId}.",
				preferenceKey, userId);

			var preference = await _userPreferenceRepository.GetByUserAndKeyAsync(userId, preferenceKey);
			if (preference == null)
			{
				throw new NotFoundException($"Preference with key '{preferenceKey}' not found for this user.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				preference.PreferenceValue = request.PreferenceValue;
				if (!string.IsNullOrWhiteSpace(request.DataType))
				{
					preference.DataType = request.DataType;
				}
				preference.UpdatedAt = DateTime.UtcNow;

				_userPreferenceRepository.Update(preference);
				await _userPreferenceRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Preference {PreferenceKey} updated successfully for user {UserId}.",
					preferenceKey, userId);

				return _mapper.Map<UserPreferenceDto>(preference);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating preference.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> DeletePreferenceAsync(int userId, string preferenceKey)
		{
			_logger.LogInformation("Deleting preference {PreferenceKey} for user {UserId}.",
				preferenceKey, userId);

			var exists = await _userPreferenceRepository.PreferenceExistsAsync(userId, preferenceKey);
			if (!exists)
			{
				throw new NotFoundException($"Preference with key '{preferenceKey}' not found for this user.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var success = await _userPreferenceRepository.DeleteUserPreferenceAsync(userId, preferenceKey);

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Preference {PreferenceKey} deleted successfully for user {UserId}.",
					preferenceKey, userId);

				return success;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting preference.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> DeleteAllPreferencesAsync(int userId)
		{
			_logger.LogInformation("Deleting all preferences for user {UserId}.", userId);

			// Validate user
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var success = await _userPreferenceRepository.DeleteAllUserPreferencesAsync(userId);

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("All preferences deleted successfully for user {UserId}.", userId);

				return success;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting all preferences.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		#endregion

		#region Read Operations

		public async Task<UserPreferenceDto?> GetPreferenceAsync(int userId, string preferenceKey)
		{
			var preference = await _userPreferenceRepository.GetByUserAndKeyAsync(userId, preferenceKey);
			if (preference == null)
			{
				return null;
			}

			return _mapper.Map<UserPreferenceDto>(preference);
		}

		public async Task<IEnumerable<UserPreferenceDto>> GetAllUserPreferencesAsync(int userId)
		{
			// Validate user
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var preferences = await _userPreferenceRepository.GetUserPreferencesAsync(userId);
			return _mapper.Map<IEnumerable<UserPreferenceDto>>(preferences);
		}

		public async Task<Dictionary<string, string>> GetPreferencesByKeysAsync(int userId, List<string> keys)
		{
			var preferences = await _userPreferenceRepository.GetByKeysAsync(userId, keys);
			return preferences.ToDictionary(p => p.PreferenceKey, p => p.PreferenceValue);
		}

		public async Task<string?> GetPreferenceValueAsync(
			int userId,
			string preferenceKey,
			string? defaultValue = null)
		{
			var preference = await _userPreferenceRepository.GetByUserAndKeyAsync(userId, preferenceKey);
			return preference?.PreferenceValue ?? defaultValue;
		}

		#endregion

		#region Bulk Operations

		public async Task<int> BulkSetPreferencesAsync(int userId, BulkUserPreferenceDto request)
		{
			_logger.LogInformation("Bulk setting {Count} preferences for user {UserId}.",
				request.Preferences.Count, userId);

			// Validate user
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			if (!request.Preferences.Any())
			{
				throw new BadRequestException("No preferences provided.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var updatedCount = await _userPreferenceRepository.BulkUpsertPreferencesAsync(
					userId, request.Preferences);

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Successfully bulk set {Count} preferences for user {UserId}.",
					updatedCount, userId);

				return updatedCount;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while bulk setting preferences.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> ResetToDefaultsAsync(int userId)
		{
			_logger.LogInformation("Resetting preferences to defaults for user {UserId}.", userId);

			// Validate user
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Delete all existing preferences
				await _userPreferenceRepository.DeleteAllUserPreferencesAsync(userId);

				// Create default preferences
				var defaultPreferences = GetDefaultPreferences();
				foreach (var kvp in defaultPreferences)
				{
					var preference = new UserPreference
					{
						UserId = userId,
						PreferenceKey = kvp.Key,
						PreferenceValue = kvp.Value,
						DataType = "string",
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
					};
					await _userPreferenceRepository.AddAsync(preference);
				}

				await _userPreferenceRepository.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Preferences reset to defaults successfully for user {UserId}.", userId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while resetting preferences.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		#endregion

		#region Typed Getters

		public async Task<bool> GetBoolPreferenceAsync(
			int userId,
			string preferenceKey,
			bool defaultValue = false)
		{
			var value = await GetPreferenceValueAsync(userId, preferenceKey);
			if (string.IsNullOrWhiteSpace(value))
			{
				return defaultValue;
			}

			return bool.TryParse(value, out var result) ? result : defaultValue;
		}

		public async Task<int> GetIntPreferenceAsync(
			int userId,
			string preferenceKey,
			int defaultValue = 0)
		{
			var value = await GetPreferenceValueAsync(userId, preferenceKey);
			if (string.IsNullOrWhiteSpace(value))
			{
				return defaultValue;
			}

			return int.TryParse(value, out var result) ? result : defaultValue;
		}

		public async Task<decimal> GetDecimalPreferenceAsync(
			int userId,
			string preferenceKey,
			decimal defaultValue = 0m)
		{
			var value = await GetPreferenceValueAsync(userId, preferenceKey);
			if (string.IsNullOrWhiteSpace(value))
			{
				return defaultValue;
			}

			return decimal.TryParse(value, out var result) ? result : defaultValue;
		}

		#endregion

		#region Private Helper Methods

		private Dictionary<string, string> GetDefaultPreferences()
		{
			return new Dictionary<string, string>
			{
				// Display preferences
				{ PreferenceKeys.Language, "en" },
				{ PreferenceKeys.Theme, "light" },
				{ PreferenceKeys.Currency, "VND" },
				{ PreferenceKeys.DateFormat, "dd/MM/yyyy" },
				{ PreferenceKeys.TimeFormat, "24h" },

				// Notification preferences
				{ PreferenceKeys.EmailNotifications, "true" },
				{ PreferenceKeys.PushNotifications, "true" },
				{ PreferenceKeys.SmsNotifications, "false" },
				{ PreferenceKeys.MarketingEmails, "true" },

				// Booking preferences
				{ PreferenceKeys.DefaultGuests, "2" },
				{ PreferenceKeys.DefaultCheckInTime, "14:00" },
				{ PreferenceKeys.DefaultCheckOutTime, "12:00" },

				// Privacy preferences
				{ PreferenceKeys.ProfileVisibility, "public" },
				{ PreferenceKeys.ShowEmail, "false" },
				{ PreferenceKeys.ShowPhone, "false" },

				// Search preferences
				{ PreferenceKeys.DefaultSearchRadius, "50" }, // km
				{ PreferenceKeys.DefaultPriceRange, "0-10000000" }
			};
		}

		#endregion
	}
}