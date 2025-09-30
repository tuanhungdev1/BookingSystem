using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
			var query = _dbSet.AsQueryable();

			if (!string.IsNullOrEmpty(filter.SearchTerm))
			{
				var searchTerm = filter.SearchTerm.ToLower();
				query = query.Where(a => a.FirstName.ToLower().Contains(searchTerm) ||
										 a.LastName.ToLower().Contains(searchTerm) ||
										 a.Email.ToLower().Contains(searchTerm) ||
										 a.UserName.ToLower().Contains(searchTerm)
										 ); 
			}

			if (filter.IsActive.HasValue)
			{
				query = query.Where(a => a.IsActive == filter.IsActive.Value);
			}

			// Apply sorting (case-insensitive for strings)
			query = filter.SortBy?.ToLower() switch
			{
				"name" => filter.SortDirection == "desc"
					? query.OrderByDescending(a => (a.FirstName + " " + a.LastName).ToLower())
					: query.OrderBy(a => (a.FirstName + " " + a.LastName).ToLower()),

				"email" => filter.SortDirection == "desc"
					? query.OrderByDescending(a => a.Email.ToLower())
					: query.OrderBy(a => a.Email.ToLower()),

				"createdat" => filter.SortDirection == "desc"
					? query.OrderByDescending(a => a.CreatedAt)
					: query.OrderBy(a => a.CreatedAt),

				_ => query.OrderBy(a => a.Email.ToLower()) // default sort by Email
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
	}
}
