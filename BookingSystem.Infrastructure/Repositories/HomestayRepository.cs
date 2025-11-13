using BookingSystem.Domain.Base;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Infrastructure.Utils;
using MailKit.Search;

namespace BookingSystem.Infrastructure.Repositories
{
	public class HomestayRepository : Repository<Homestay>, IHomestayRepository
	{
		public HomestayRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<Homestay>> GetHomestaysByOwnerIdAsync(int ownerId)
		{
			return await _context.Homestays
				.Where(h => h.OwnerId == ownerId && !h.IsDeleted)
				.ToListAsync();
		}

		public async Task<IEnumerable<Homestay>> GetHomestaysByHostIdAsync(int hostId)
		{
			return await _context.Homestays
				.Where(h => h.OwnerId == hostId && !h.IsDeleted)
				.ToListAsync();
		}

		// ✅ HÀM MỚI: Lấy thông tin chi tiết homestay qua Slug
		public async Task<Homestay?> GetHomestayBySlugAsync(string slug)
		{
			if (string.IsNullOrWhiteSpace(slug))
				return null;

			var homestay = await _dbSet
				.Include(h => h.Owner)
				.Include(h => h.PropertyType)
				.Include(h => h.HomestayImages.OrderBy(img => img.DisplayOrder))
				.Include(h => h.HomestayAmenities)
					.ThenInclude(ha => ha.Amenity)
				.Include(h => h.HomestayRules)
					.ThenInclude(hr => hr.Rule)
				.Include(h => h.Reviews.Where(r => r.IsVisible))
					.ThenInclude(r => r.Reviewer)
				.Include(h => h.AvailabilityCalendars.Where(ac => ac.AvailableDate >= DateOnly.FromDateTime(DateTime.UtcNow)))
				.FirstOrDefaultAsync(h => h.Slug == slug.ToLower());

			// Tăng view count
			if (homestay != null)
			{
				homestay.ViewCount++;
				await _context.SaveChangesAsync();
			}

			return homestay;
		}

		public async Task<Homestay?> GetHomestayByIdAsync(int id)
		{
			return await _dbSet
				.Include(h => h.Owner)
				.Include(h => h.AvailabilityCalendars)
				.Include(h => h.PropertyType)
				.Include(h => h.HomestayImages)
				.Include(h => h.HomestayAmenities)
					.ThenInclude(ha => ha.Amenity)
				.Include(h => h.HomestayRules)
					.ThenInclude(hr => hr.Rule)
				.FirstOrDefaultAsync(h => h.Id == id);
		}

		public async Task<PagedResult<Homestay>> GetAllHomestayAsync(HomestayFilter filter)
		{
			var query = _dbSet.AsQueryable();

			if (!string.IsNullOrEmpty(filter.Search))
			{
				var searchTerm = filter.Search.RemoveDiacritics().ToLower();

				query = query.Where(h =>
					h.HomestayTitleNormalized.ToLower().Contains(searchTerm) ||
					(h.HomestayDescriptionNormalized != null &&
					 h.HomestayDescriptionNormalized.ToLower().Contains(searchTerm)) ||
					h.FullAddressNormalized.ToLower().Contains(searchTerm) ||
					h.CountryNormalized.ToLower().Contains(searchTerm) ||
					h.CityNormalized.ToLower().Contains(searchTerm) ||
					(h.SearchKeywordsNormalized != null &&
					 h.SearchKeywordsNormalized.ToLower().Contains(searchTerm)) ||
					(h.SlugNormalized != null &&
					 h.SlugNormalized.ToLower().Contains(searchTerm))
				);
			}

			// ===== LOCATION FILTERS =====
			if (!string.IsNullOrEmpty(filter.City))
			{
				var city = filter.City.RemoveDiacritics().ToLower();
				query = query.Where(h => h.CityNormalized.ToLower().Contains(city));
			}

			if (!string.IsNullOrEmpty(filter.Province))
			{
				var province = filter.Province.RemoveDiacritics().ToLower();
				query = query.Where(h => h.ProvinceNormalized.ToLower().Contains(province));
			}

			if (!string.IsNullOrEmpty(filter.Country))
			{
				var country = filter.Country.RemoveDiacritics().ToLower();
				query = query.Where(h => h.CountryNormalized.ToLower().Contains(country));
			}

			// Radius search (if coordinates provided)
			if (filter.Latitude.HasValue && filter.Longitude.HasValue && filter.RadiusInKm.HasValue)
			{
				var latDelta = (double)(filter.RadiusInKm.Value / 111m);
				var lonDelta = (double)(filter.RadiusInKm.Value / (111m * (decimal)Math.Cos((double)filter.Latitude.Value * Math.PI / 180)));

				query = query.Where(h =>
					h.Latitude >= filter.Latitude.Value - (decimal)latDelta &&
					h.Latitude <= filter.Latitude.Value + (decimal)latDelta &&
					h.Longitude >= filter.Longitude.Value - (decimal)lonDelta &&
					h.Longitude <= filter.Longitude.Value + (decimal)lonDelta);
			}

			if (filter.PropertyTypeId.HasValue)
			{
				query = query.Where(h => h.PropertyTypeId == filter.PropertyTypeId.Value);
			}

			if (!string.IsNullOrEmpty(filter.PropertyTypeIds))
			{
				var propertyTypeIds = filter.PropertyTypeIds
					.Split(',', StringSplitOptions.RemoveEmptyEntries)
					.Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
					.Where(id => id > 0)
					.ToList();

				if (propertyTypeIds.Any())
				{
					query = query.Where(h => propertyTypeIds.Contains(h.PropertyTypeId));
				}
			}

			if (filter.Adults.HasValue)
			{
				query = query.Where(h => h.MaximumGuests >= filter.Adults.Value);
			}

			if (filter.Children.HasValue)
			{
				query = query.Where(h => h.MaximumChildren >= filter.Children.Value);
			}

			if (filter.Rooms.HasValue)
			{
				query = query.Where(h => h.NumberOfRooms >= filter.Rooms.Value);
			}

			if (filter.Pets.HasValue && filter.Pets.Value)
			{
				query = query.Where(h => h.IsPetFriendly);
			}

			// Property type filter
			//if (!string.IsNullOrEmpty(filter.Type))
			//{
			//	query = query.Where(h => h.PropertyType.TypeName == filter.Type);
			//}

			if (filter.PropertyTypeId.HasValue)
			{
				query = query.Where(h => h.PropertyTypeId == filter.PropertyTypeId.Value);
			}

			// Status filters
			if (filter.IsActive.HasValue)
			{
				query = query.Where(h => h.IsActive == filter.IsActive.Value);
			}

			if (filter.IsApproved.HasValue)
			{
				query = query.Where(h => h.IsApproved == filter.IsApproved.Value);
			}

			if (filter.IsFeatured.HasValue)
			{
				query = query.Where(h => h.IsFeatured == filter.IsFeatured.Value);
			}

			if (filter.IsInstantBook.HasValue)
			{
				query = query.Where(h => h.IsInstantBook == filter.IsInstantBook.Value);
			}

			// Owner filter
			if (filter.OwnerId.HasValue)
			{
				query = query.Where(h => h.OwnerId == filter.OwnerId.Value);
			}

			if (filter.MinRating.HasValue)
			{
				query = query.Where(h => h.RatingAverage >= filter.MinRating.Value);
			}

			// Price range
			if (filter.MinPrice.HasValue)
			{
				query = query.Where(h => h.BaseNightlyPrice >= filter.MinPrice.Value);
			}

			if (filter.MaxPrice.HasValue)
			{
				query = query.Where(h => h.BaseNightlyPrice <= filter.MaxPrice.Value);
			}

			if (filter.HasWeekendPrice.HasValue)
			{
				query = filter.HasWeekendPrice.Value
					? query.Where(h => h.WeekendPrice.HasValue)
					: query.Where(h => !h.WeekendPrice.HasValue);
			}

			if (filter.HasWeeklyDiscount.HasValue)
			{
				query = filter.HasWeeklyDiscount.Value
					? query.Where(h => h.WeeklyDiscount.HasValue)
					: query.Where(h => !h.WeeklyDiscount.HasValue);
			}

			if (filter.HasMonthlyDiscount.HasValue)
			{
				query = filter.HasMonthlyDiscount.Value
					? query.Where(h => h.MonthlyDiscount.HasValue)
					: query.Where(h => !h.MonthlyDiscount.HasValue);
			}

			// Guest capacity
			if (filter.MinGuests.HasValue)
			{
				query = query.Where(h => h.MaximumGuests >= filter.MinGuests.Value);
			}

			// ✅ THÊM MỚI: Children capacity
			if (filter.MinChildren.HasValue)
			{
				query = query.Where(h => h.MaximumChildren >= filter.MinChildren.Value);
			}

			// Room requirements
			if (filter.MinBedrooms.HasValue)
			{
				query = query.Where(h => h.NumberOfBedrooms >= filter.MinBedrooms.Value);
			}

			if (filter.MinBathrooms.HasValue)
			{
				query = query.Where(h => h.NumberOfBathrooms >= filter.MinBathrooms.Value);
			}

			if (filter.MinBeds.HasValue)
			{
				query = query.Where(h => h.NumberOfBeds >= filter.MinBeds.Value);
			}

			// ✅ THÊM MỚI: Total rooms filter
			if (filter.MinRooms.HasValue)
			{
				query = query.Where(h => h.NumberOfRooms >= filter.MinRooms.Value);
			}

			// ✅ THÊM MỚI: Amenities / features filters
			if (filter.HasParking.HasValue)
			{
				query = query.Where(h => h.HasParking == filter.HasParking.Value);
			}

			if (filter.IsPetFriendly.HasValue)
			{
				query = query.Where(h => h.IsPetFriendly == filter.IsPetFriendly.Value);
			}

			if (filter.HasPrivatePool.HasValue)
			{
				query = query.Where(h => h.HasPrivatePool == filter.HasPrivatePool.Value);
			}

			// Date filters
			if (filter.CreatedFrom.HasValue)
			{
				query = query.Where(h => h.CreatedAt >= filter.CreatedFrom.Value);
			}

			if (filter.CreatedTo.HasValue)
			{
				query = query.Where(h => h.CreatedAt <= filter.CreatedTo.Value);
			}

			if (filter.ApprovedFrom.HasValue)
			{
				query = query.Where(h => h.ApprovedAt >= filter.ApprovedFrom.Value);
			}

			if (filter.ApprovedTo.HasValue)
			{
				query = query.Where(h => h.ApprovedAt <= filter.ApprovedTo.Value);
			}

			// Amenities filter (homestay must have ALL specified amenities)
			if (!string.IsNullOrEmpty(filter.AmenityIds))
			{
				var amenityIds = filter.AmenityIds
					.Split(',', StringSplitOptions.RemoveEmptyEntries)
					.Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
					.Where(id => id > 0)
					.ToList();

				if (amenityIds.Any())
				{
					query = query.Where(h => amenityIds.All(amenityId =>
						h.HomestayAmenities.Any(ha => ha.AmenityId == amenityId)));
				}
			}

			// Availability date range filter
			if (filter.CheckInDate.HasValue && filter.CheckOutDate.HasValue)
			{
				query = query.Where(h =>
					!h.AvailabilityCalendars.Any(ac =>
						ac.AvailableDate >= filter.CheckInDate.Value &&
						ac.AvailableDate < filter.CheckOutDate.Value &&
						(!ac.IsAvailable || ac.IsBlocked)));
			}

			// Apply sorting
			query = filter.SortBy?.ToLower() switch
			{
				"name" or "title" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.HomestayTitle)
					: query.OrderBy(h => h.HomestayTitle),

				"price" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.BaseNightlyPrice)
					: query.OrderBy(h => h.BaseNightlyPrice),

				"guests" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.MaximumGuests)
					: query.OrderBy(h => h.MaximumGuests),

				"rating" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.RatingAverage)
					: query.OrderBy(h => h.RatingAverage),

				"reviews" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.TotalReviews)
					: query.OrderBy(h => h.TotalReviews),

				"bookings" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.BookingCount)
					: query.OrderBy(h => h.BookingCount),

				"views" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.ViewCount)
					: query.OrderBy(h => h.ViewCount),

				"createdat" or "created" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.CreatedAt)
					: query.OrderBy(h => h.CreatedAt),

				"updatedat" or "updated" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.UpdatedAt)
					: query.OrderBy(h => h.UpdatedAt),

				"approvedat" or "approved" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.ApprovedAt)
					: query.OrderBy(h => h.ApprovedAt),

				"featured" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.IsFeatured)
					: query.OrderBy(h => h.IsFeatured),
				"area" => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.AreaInSquareMeters ?? 0)
					: query.OrderBy(h => h.AreaInSquareMeters ?? 0),
				"popular" => query.OrderByDescending(h => h.ViewCount)
					.ThenByDescending(h => h.BookingCount)
					.ThenByDescending(h => h.RatingAverage),
				"newest" or "latest" => query.OrderByDescending(h => h.CreatedAt),
				"oldest" => query.OrderBy(h => h.CreatedAt),
				_ => filter.SortDirection?.ToLower() == "desc"
					? query.OrderByDescending(h => h.CreatedAt)
					: query.OrderBy(h => h.CreatedAt)
			};

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.Include(h => h.Owner)
				.Include(h => h.PropertyType)
				.Include(h => h.HomestayImages)
				.Include(h => h.HomestayAmenities)
					.ThenInclude(ha => ha.Amenity)
				.Include(h => h.HomestayRules)
					.ThenInclude(hr => hr.Rule)
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

		public async Task<IEnumerable<Homestay>> GetPendingApprovalsAsync()
		{
			return await _dbSet
				.Where(h => !h.IsApproved && !h.IsDeleted)
				.Include(h => h.Owner)
				.Include(h => h.PropertyType)
				.Include(h => h.HomestayImages)
				.OrderBy(h => h.CreatedAt)
				.ToListAsync();
		}

		public async Task<int> CountPendingApprovalsAsync()
		{
			return await _dbSet
				.Where(h => !h.IsApproved && !h.IsDeleted)
				.CountAsync();
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
