using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Infrastructure.Repositories
{
	public class HomestayRepository : Repository<Homestay>, IHomestayRepository
	{
		public HomestayRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<PagedResult<Homestay>> GetPagedAsync(HomestayFilter filter)
		{
			var query = _dbSet.AsQueryable();

			if (!string.IsNullOrEmpty(filter.SearchTerm))
			{
				var searchTerm = filter.SearchTerm.ToLower();
				query = query.Where(a => a.HomestayTitle.ToLower().Contains(searchTerm) ||
										 a.HomestayDescription.ToLower().Contains(searchTerm) ||
										 a.FullAddress.ToLower().Contains(searchTerm));
			}

			if (!string.IsNullOrEmpty(filter.City))
			{
				var city = filter.City.ToLower();
				query = query.Where(a => a.City.ToLower().Contains(city));
			}

			if (!string.IsNullOrEmpty(filter.Country))
			{
				var country = filter.Country.ToLower();
				query = query.Where(a => a.Country.ToLower().Contains(country));
			}

			if (!string.IsNullOrEmpty(filter.Type))
			{
				query = query.Where(a => a.PropertyType.TypeName == filter.Type);
			}

			if (filter.IsActive.HasValue)
			{
				query = query.Where(a => a.IsActive == filter.IsActive.Value);
			}

			// Apply sorting (case-insensitive for strings)
			query = filter.SortBy?.ToLower() switch
			{
				"name" => filter.SortDirection == "desc" ? query.OrderByDescending(a => a.HomestayTitle.ToLower()) : query.OrderBy(a => a.HomestayTitle.ToLower()),
				"createdat" => filter.SortDirection == "desc" ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
				_ => query.OrderBy(a => a.HomestayTitle.ToLower())
			};

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.Include(a => a.HomestayImages)
				.Include(a => a.HomestayAmenities)
				.Include(a => a.HomestayRules)
				.ToListAsync();

			return new PagedResult<Homestay>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize,
				TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
			};
		}


		public async Task<IEnumerable<Homestay>> SearchAsync(string searchTerm)
		{
			if (string.IsNullOrEmpty(searchTerm))
				return await GetAllAsync();

			return await _dbSet
				.Where(a => a.HomestayTitle.Contains(searchTerm) ||
						   a.HomestayDescription.Contains(searchTerm) ||
						   a.FullAddress.Contains(searchTerm) ||
						   a.City.Contains(searchTerm))
				.ToListAsync();
		}

		public async Task<IEnumerable<Homestay>> GetByLocationAsync(string city, string country)
		{
			return await _dbSet
				.Where(a => a.City.Contains(city) && a.Country.Contains(country))
				.ToListAsync();
		}

		public async Task<IEnumerable<Homestay>> GetByTypeAsync(int HomestayTypeId)
		{
			return await _dbSet
				.Where(a => a.PropertyType.Id == HomestayTypeId && a.IsActive)
				.ToListAsync();
		}

		public async Task<Homestay?> GetWithDetailsAsync(int id)
		{
			return await _dbSet
				.FirstOrDefaultAsync(a => a.Id == id);
		}

		public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
		{
			var query = _dbSet.Where(a => a.HomestayTitle == name);

			if (excludeId.HasValue)
				query = query.Where(a => a.Id != excludeId.Value);

			return await query.AnyAsync();
		}
	}
}
