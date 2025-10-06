using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;

namespace BookingSystem.Infrastructure.Repositories
{
	public class WishlistItemRepository : Repository<WishlistItem>, IWishlistItemRepository
	{
		public WishlistItemRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<PagedResult<WishlistItem>> GetWishlistByUserIdAsync(int userId, WishlistFilter filter)
		{
			var query = _dbSet
				.Include(w => w.Homestay)
					.ThenInclude(h => h.PropertyType)
				.Include(w => w.Homestay.HomestayImages)
				.Include(w => w.Homestay.Owner)
				.Where(w => w.UserId == userId);

			// Apply filters
			if (!string.IsNullOrWhiteSpace(filter.HomestayName))
			{
				query = query.Where(w => w.Homestay.HomestayTitle.ToLower().Contains(filter.HomestayName.ToLower()));
			}

			if (!string.IsNullOrWhiteSpace(filter.Location))
			{
				query = query.Where(w =>
					w.Homestay.City.Contains(filter.Location) ||
					w.Homestay.Country.Contains(filter.Location) ||
					w.Homestay.Province.Contains(filter.Location));
			}


			if (filter.PropertyTypeId.HasValue)
			{
				query = query.Where(w => w.Homestay.PropertyTypeId == filter.PropertyTypeId.Value);
			}

			// Only show active and approved homestays
			query = query.Where(w => w.Homestay.IsActive && w.Homestay.IsApproved);

			// Get total count
			var totalCount = await query.CountAsync();

			// Apply sorting
			query = filter.sortBy?.ToLower() switch
			{
				"name" => filter.sortOrder?.ToLower() == "desc"
					? query.OrderByDescending(w => w.Homestay.HomestayTitle)
					: query.OrderBy(w => w.Homestay.HomestayTitle),
				"createdat" => filter.sortOrder?.ToLower() == "desc"
					? query.OrderByDescending(w => w.CreatedAt)
					: query.OrderBy(w => w.CreatedAt),
				_ => query.OrderByDescending(w => w.CreatedAt)
			};

			// Apply pagination
			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			return new PagedResult<WishlistItem>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize
			};
		}

		public async Task<WishlistItem?> GetByUserAndHomestayAsync(int userId, int homestayId)
		{
			return await _dbSet
				.Include(w => w.Homestay)
					.ThenInclude(h => h.PropertyType)
				.Include(w => w.Homestay.HomestayImages)
				.Include(w => w.Homestay.Owner)
				.FirstOrDefaultAsync(w => w.UserId == userId && w.HomestayId == homestayId);
		}

		public async Task<bool> ExistsAsync(int userId, int homestayId)
		{
			return await _dbSet.AnyAsync(w => w.UserId == userId && w.HomestayId == homestayId);
		}

		public async Task<int> GetWishlistCountByUserIdAsync(int userId)
		{
			return await _dbSet.CountAsync(w => w.UserId == userId);
		}

		public async Task<IEnumerable<WishlistItem>> GetWishlistWithDetailsAsync(int userId)
		{
			return await _dbSet
				.Include(w => w.Homestay)
					.ThenInclude(h => h.PropertyType)
				.Include(w => w.Homestay.HomestayImages)
				.Include(w => w.Homestay.Owner)
				.Include(w => w.Homestay.HomestayAmenities)
					.ThenInclude(ha => ha.Amenity)
				.Where(w => w.UserId == userId && w.Homestay.IsActive && w.Homestay.IsApproved)
				.OrderByDescending(w => w.CreatedAt)
				.ToListAsync();
		}
	}
}