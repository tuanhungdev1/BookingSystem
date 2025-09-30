using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;

namespace BookingSystem.Infrastructure.Repositories
{
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
	{
		public AmenityRepository(BookingDbContext context) : base(context)
		{

		}

		public async Task<IEnumerable<Amenity>> GetPopularAmenitiesAsync(int count)
		{
			return await Task.FromResult(_context.Amenities
				.Where(a => a.IsActive)
				.OrderByDescending(a => a.UsageCount) 
				.Take(count)
				.ToList());
		}

		public async Task<PagedResult<Amenity>> GetPagedAsync(AmenityFilter amenityFilter)
		{
			var query = _dbSet.AsQueryable();
			if (!string.IsNullOrEmpty(amenityFilter.SearchTerm))
			{
				var searchTerm = amenityFilter.SearchTerm.ToLower();
				query = query.Where(a => a.AmenityName.ToLower().Contains(searchTerm) ||
										 a.AmenityDescription.ToLower().Contains(searchTerm) ||
										 a.Category.ToLower().Contains(searchTerm));
			}
			if (!string.IsNullOrEmpty(amenityFilter.Category))
			{
				var category = amenityFilter.Category.ToLower();
				query = query.Where(a => a.Category.ToLower().Contains(category));
			}
			if (amenityFilter.IsActive.HasValue)
			{
				query = query.Where(a => a.IsActive == amenityFilter.IsActive.Value);
			}
			query = amenityFilter.SortBy?.ToLower() switch
			{
				"name" => amenityFilter.SortDirection == "desc" ? query.OrderByDescending(a => a.AmenityName.ToLower()) : query.OrderBy(a => a.AmenityName.ToLower()),
				"createdat" => amenityFilter.SortDirection == "desc" ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
				_ => query.OrderBy(a => a.AmenityName.ToLower())
			};
			var totalCount = await Task.FromResult(query.Count());
			var items = await Task.FromResult(query
				.Skip((amenityFilter.PageNumber - 1) * amenityFilter.PageSize)
				.Take(amenityFilter.PageSize)
				.ToList());
			return new PagedResult<Amenity>(items, totalCount, amenityFilter.PageNumber, amenityFilter.PageSize);
		}

		public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
		{
			var query = _dbSet.AsQueryable().Where(a => a.AmenityName.ToLower() == name.ToLower());
			if (excludeId.HasValue)
			{
				query = query.Where(a => a.Id != excludeId.Value);
			}
			return await Task.FromResult(query.Any());
		}

		public async Task<IEnumerable<Amenity>> GetActiveAmenitiesAsync()
		{
			return await Task.FromResult(_dbSet.Where(a => a.IsActive).ToList());
		}

		public async Task<IEnumerable<Amenity>> GetAmenitiesByCategoryAsync(string category)
		{
			return await Task.FromResult(_dbSet.Where(a => a.Category.ToLower() == category.ToLower() && a.IsActive).ToList());
		}

		public async Task<IEnumerable<string>> GetAllCategoriesAsync()
		{
			return await Task.FromResult(_dbSet
				.Where(a => !string.IsNullOrEmpty(a.Category))
				.Select(a => a.Category)
				.Distinct()
				.ToList());
		}

		public async Task<int> GetTotalAmenitiesCountAsync()
		{
			return await Task.FromResult(_dbSet.Count());
		}

		public async Task<int> GetActiveAmenitiesCountAsync()
		{
			return await Task.FromResult(_dbSet.Count(a => a.IsActive));
		}
	}
}
