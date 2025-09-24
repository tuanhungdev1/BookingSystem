using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Infrastructure.Repositories
{
	public class AccommodationRepository : Repository<Accommodation>, IAccommodationRepository
	{
		public AccommodationRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<PagedResult<Accommodation>> GetPagedAsync(AccommodationFilter filter)
		{
			var query = _dbSet.AsQueryable();

			if (!string.IsNullOrEmpty(filter.SearchTerm))
			{
				var searchTerm = filter.SearchTerm.ToLower();
				query = query.Where(a => a.Name.ToLower().Contains(searchTerm) ||
										 a.Description.ToLower().Contains(searchTerm) ||
										 a.Address.ToLower().Contains(searchTerm));
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
				query = query.Where(a => a.Type.Name == filter.Type);
			}

			if (filter.MinStarRating.HasValue)
			{
				query = query.Where(a => a.StarRating >= filter.MinStarRating.Value);
			}

			if (filter.MaxStarRating.HasValue)
			{
				query = query.Where(a => a.StarRating <= filter.MaxStarRating.Value);
			}

			if (filter.IsActive.HasValue)
			{
				query = query.Where(a => a.IsActive == filter.IsActive.Value);
			}

			// Apply sorting (case-insensitive for strings)
			query = filter.SortBy?.ToLower() switch
			{
				"name" => filter.SortDirection == "desc" ? query.OrderByDescending(a => a.Name.ToLower()) : query.OrderBy(a => a.Name.ToLower()),
				"city" => filter.SortDirection == "desc" ? query.OrderByDescending(a => a.City.ToLower()) : query.OrderBy(a => a.City.ToLower()),
				"starrating" => filter.SortDirection == "desc" ? query.OrderByDescending(a => a.StarRating) : query.OrderBy(a => a.StarRating),
				"createdat" => filter.SortDirection == "desc" ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
				_ => query.OrderBy(a => a.Name.ToLower())
			};

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.Include(a => a.RoomTypes)
				.Include(a => a.HotelAmenities)
				.ToListAsync();

			return new PagedResult<Accommodation>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize,
				TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
			};
		}


		public async Task<IEnumerable<Accommodation>> SearchAsync(string searchTerm)
		{
			if (string.IsNullOrEmpty(searchTerm))
				return await GetAllAsync();

			return await _dbSet
				.Where(a => a.Name.Contains(searchTerm) ||
						   a.Description.Contains(searchTerm) ||
						   a.Address.Contains(searchTerm) ||
						   a.City.Contains(searchTerm))
				.Include(a => a.RoomTypes)
				.ToListAsync();
		}

		public async Task<IEnumerable<Accommodation>> GetByLocationAsync(string city, string country)
		{
			return await _dbSet
				.Where(a => a.City.Contains(city) && a.Country.Contains(country))
				.Include(a => a.RoomTypes)
				.ToListAsync();
		}

		public async Task<IEnumerable<Accommodation>> GetByTypeAsync(Guid AccommodationTypeId)
		{
			return await _dbSet
				.Where(a => a.Type.Id == AccommodationTypeId && a.IsActive)
				.Include(a => a.RoomTypes)
				.ToListAsync();
		}

		public async Task<Accommodation?> GetWithDetailsAsync(Guid id)
		{
			return await _dbSet
				.Include(a => a.RoomTypes)
				.Include(a => a.Rooms)
				.Include(a => a.HotelAmenities)
				.Include(a => a.HotelImages)
				.FirstOrDefaultAsync(a => a.Id == id);
		}

		public async Task<bool> IsNameExistsAsync(string name, Guid? excludeId = null)
		{
			var query = _dbSet.Where(a => a.Name == name);

			if (excludeId.HasValue)
				query = query.Where(a => a.Id != excludeId.Value);

			return await query.AnyAsync();
		}
	}
}
