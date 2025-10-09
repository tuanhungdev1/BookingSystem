using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories
{
	public class UserPreferenceRepository : Repository<UserPreference>, IUserPreferenceRepository
	{
		public UserPreferenceRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<UserPreference?> GetByIdWithDetailsAsync(int id)
		{
			return await _dbSet
				.Include(up => up.User)
				.FirstOrDefaultAsync(up => up.Id == id);
		}

		public async Task<UserPreference?> GetByUserAndKeyAsync(int userId, string preferenceKey)
		{
			return await _dbSet
				.Include(up => up.User)
				.FirstOrDefaultAsync(up => up.UserId == userId && up.PreferenceKey == preferenceKey);
		}

		public async Task<IEnumerable<UserPreference>> GetUserPreferencesAsync(int userId)
		{
			return await _dbSet
				.Where(up => up.UserId == userId)
				.OrderBy(up => up.PreferenceKey)
				.ToListAsync();
		}

		public async Task<IEnumerable<UserPreference>> GetByKeysAsync(int userId, List<string> keys)
		{
			return await _dbSet
				.Where(up => up.UserId == userId && keys.Contains(up.PreferenceKey))
				.ToListAsync();
		}

		public async Task<bool> PreferenceExistsAsync(int userId, string preferenceKey)
		{
			return await _dbSet
				.AnyAsync(up => up.UserId == userId && up.PreferenceKey == preferenceKey);
		}

		public async Task<bool> DeleteUserPreferenceAsync(int userId, string preferenceKey)
		{
			var preference = await _dbSet
				.FirstOrDefaultAsync(up => up.UserId == userId && up.PreferenceKey == preferenceKey);

			if (preference == null)
				return false;

			_dbSet.Remove(preference);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteAllUserPreferencesAsync(int userId)
		{
			var preferences = await _dbSet
				.Where(up => up.UserId == userId)
				.ToListAsync();

			if (!preferences.Any())
				return false;

			_dbSet.RemoveRange(preferences);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<int> BulkUpsertPreferencesAsync(int userId, Dictionary<string, string> preferences)
		{
			var existingPreferences = await _dbSet
				.Where(up => up.UserId == userId && preferences.Keys.Contains(up.PreferenceKey))
				.ToListAsync();

			var existingKeys = existingPreferences.Select(p => p.PreferenceKey).ToHashSet();
			var updatedCount = 0;

			// Update existing preferences
			foreach (var existing in existingPreferences)
			{
				if (preferences.TryGetValue(existing.PreferenceKey, out var newValue))
				{
					existing.PreferenceValue = newValue;
					existing.UpdatedAt = DateTime.UtcNow;
					_dbSet.Update(existing);
					updatedCount++;
				}
			}

			// Add new preferences
			foreach (var kvp in preferences)
			{
				if (!existingKeys.Contains(kvp.Key))
				{
					var newPreference = new UserPreference
					{
						UserId = userId,
						PreferenceKey = kvp.Key,
						PreferenceValue = kvp.Value,
						DataType = "string",
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
					};
					await _dbSet.AddAsync(newPreference);
					updatedCount++;
				}
			}

			await _context.SaveChangesAsync();
			return updatedCount;
		}
	}
}