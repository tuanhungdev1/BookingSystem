using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
	{
		public UserRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
		{
			return await _context.Users
				.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
										u.RefreshTokenExpiryTime > DateTime.UtcNow);
		}

		public async Task<PagedResult<User>> GetPagedAsync(UserFilter filter)
		{
			var query = _dbSet
				.Include(u => u.UserRoles)
				.ThenInclude(ur => ur.Role)
				.AsQueryable();

			// Text Search
			if (!string.IsNullOrEmpty(filter.Search))
			{
				var searchTerm = filter.Search.ToLower();
				query = query.Where(a => a.FirstName.ToLower().Contains(searchTerm) ||
										 a.LastName.ToLower().Contains(searchTerm) ||
										 a.Email.ToLower().Contains(searchTerm) ||
										 a.UserName.ToLower().Contains(searchTerm) ||
										 (a.FirstName + " " + a.LastName).ToLower().Contains(searchTerm)
										 )
					;
			}

			// Status Filters - Parse string to bool if not "all"
			if (!string.IsNullOrEmpty(filter.IsActive) && filter.IsActive != "all")
			{
				bool isActive = bool.Parse(filter.IsActive);
				query = query.Where(a => a.IsActive == isActive);
			}

			if (!string.IsNullOrEmpty(filter.IsLocked) && filter.IsLocked != "all")
			{
				bool isLocked = bool.Parse(filter.IsLocked);
				query = query.Where(a => a.IsLocked == isLocked);
			}

			if (!string.IsNullOrEmpty(filter.IsEmailConfirmed) && filter.IsEmailConfirmed != "all")
			{
				bool isEmailConfirmed = bool.Parse(filter.IsEmailConfirmed);
				query = query.Where(a => a.IsEmailConfirmed == isEmailConfirmed);
			}

			// UserRepository.cs
			if (filter.Roles != null && filter.Roles.Length > 0)
			{
				// Normalize input roles to uppercase
				var normalizedRoles = filter.Roles
					.Select(r => r.ToUpper())
					.ToList();

				query = query.Where(u => u.UserRoles
					.Any(ur => normalizedRoles.Contains(ur.Role.NormalizedName)));
				// Sử dụng NormalizedName thay vì Name
			}

			// Date Range Filters - Parse ISO strings to DateTime
			if (!string.IsNullOrEmpty(filter.CreatedAtFrom))
			{
				if (DateTime.TryParse(filter.CreatedAtFrom, out DateTime createdFrom))
				{
					query = query.Where(a => a.CreatedAt >= createdFrom);
				}
			}

			if (!string.IsNullOrEmpty(filter.CreatedAtTo))
			{
				if (DateTime.TryParse(filter.CreatedAtTo, out DateTime createdTo))
				{
					query = query.Where(a => a.CreatedAt <= createdTo);
				}
			}

			query = query.Where(u => !u.IsDeleted);

			// Apply sorting (case-insensitive for strings)
			query = filter.SortBy?.ToLower() switch
			{
				"username" => filter.SortOrder == "desc"
					? query.OrderByDescending(a => a.UserName.ToLower())
					: query.OrderBy(a => a.UserName.ToLower()),

				"email" => filter.SortOrder == "desc"
					? query.OrderByDescending(a => a.Email.ToLower())
					: query.OrderBy(a => a.Email.ToLower()),

				"fullname" => filter.SortOrder == "desc"
					? query.OrderByDescending(a => (a.FirstName + " " + a.LastName).ToLower())
					: query.OrderBy(a => (a.FirstName + " " + a.LastName).ToLower()),

				"createdat" => filter.SortOrder == "desc"
					? query.OrderByDescending(a => a.CreatedAt)
					: query.OrderBy(a => a.CreatedAt),

				_ => query.OrderBy(a => a.UserName.ToLower()) // default sort by UserName
			};

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<User>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize,
				TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
			};
		}

		public async Task<int> CountActiveUsersAsync(DateTime since)
		{
			return await _dbSet
				.CountAsync(u => !u.IsDeleted && u.LastLoginAt >= since);
		}

		public async Task<int> CountUsersByCreatedDateAsync(DateTime startDate, DateTime endDate)
		{
			return await _dbSet
				.CountAsync(u => !u.IsDeleted && u.CreatedAt >= startDate && u.CreatedAt < endDate);
		}
	}
}
