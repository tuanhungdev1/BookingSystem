using BookingSystem.Application.DTOs.UserPreferenceDTO;

namespace BookingSystem.Application.Contracts
{
	public interface IUserPreferenceService
	{
		// CRUD Operations
		Task<UserPreferenceDto> CreatePreferenceAsync(int userId, CreateUserPreferenceDto request);
		Task<UserPreferenceDto?> UpdatePreferenceAsync(int userId, string preferenceKey, UpdateUserPreferenceDto request);
		Task<bool> DeletePreferenceAsync(int userId, string preferenceKey);
		Task<bool> DeleteAllPreferencesAsync(int userId);

		// Read Operations
		Task<UserPreferenceDto?> GetPreferenceAsync(int userId, string preferenceKey);
		Task<IEnumerable<UserPreferenceDto>> GetAllUserPreferencesAsync(int userId);
		Task<Dictionary<string, string>> GetPreferencesByKeysAsync(int userId, List<string> keys);
		Task<string?> GetPreferenceValueAsync(int userId, string preferenceKey, string? defaultValue = null);

		// Bulk Operations
		Task<int> BulkSetPreferencesAsync(int userId, BulkUserPreferenceDto request);
		Task<bool> ResetToDefaultsAsync(int userId);

		// Typed Getters (Helper methods)
		Task<bool> GetBoolPreferenceAsync(int userId, string preferenceKey, bool defaultValue = false);
		Task<int> GetIntPreferenceAsync(int userId, string preferenceKey, int defaultValue = 0);
		Task<decimal> GetDecimalPreferenceAsync(int userId, string preferenceKey, decimal defaultValue = 0m);
	}
}
