using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Repositories
{
	public interface IUserPreferenceRepository : IRepository<UserPreference>
	{
		Task<UserPreference?> GetByIdWithDetailsAsync(int id);
		Task<UserPreference?> GetByUserAndKeyAsync(int userId, string preferenceKey);
		Task<IEnumerable<UserPreference>> GetUserPreferencesAsync(int userId);
		Task<IEnumerable<UserPreference>> GetByKeysAsync(int userId, List<string> keys);
		Task<bool> PreferenceExistsAsync(int userId, string preferenceKey);
		Task<bool> DeleteUserPreferenceAsync(int userId, string preferenceKey);
		Task<bool> DeleteAllUserPreferencesAsync(int userId);
		Task<int> BulkUpsertPreferencesAsync(int userId, Dictionary<string, string> preferences);
	}
}
