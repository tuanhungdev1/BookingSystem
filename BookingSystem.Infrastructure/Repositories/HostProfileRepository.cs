using BookingSystem.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Infrastructure.Repositories
{
    public class HostProfileRepository : Repository<HostProfile>, IHostProfileRepository
	{
		public HostProfileRepository(BookingDbContext dbContext) : base(dbContext)
		{
		
		}

		public async Task<HostProfile?> GetByUserIdAsync(int userId)
		{
			return await _dbSet.FirstOrDefaultAsync(hp => hp.UserId == userId);
		}

		public async Task<PagedResult<HostProfile>> GetPagedHostProfilesAsync(HostProfileFilter hostProfileFilter)
		{
			var query = _dbSet.AsQueryable();
			// Apply filters
			if (hostProfileFilter.HostStatus.HasValue)
			{
				query = query.Where(hp => hp.Status == hostProfileFilter.HostStatus.Value);
			}
			if (hostProfileFilter.IsActive.HasValue)
			{
				query = query.Where(hp => hp.IsActive == hostProfileFilter.IsActive.Value);
			}
			if (hostProfileFilter.IsSuperhost.HasValue)
			{
				query = query.Where(hp => hp.IsSuperhost == hostProfileFilter.IsSuperhost.Value);
			}
			if (!string.IsNullOrEmpty(hostProfileFilter.SearchTerm))
			{
				var searchTerm = hostProfileFilter.SearchTerm.ToLower();
				query = query.Where(hp => hp.BusinessName.ToLower().Contains(searchTerm) ||
										  hp.AboutMe.ToLower().Contains(searchTerm) ||
										  hp.Languages.ToLower().Contains(searchTerm));
			}

			if (hostProfileFilter.RegisteredFrom.HasValue)
			{
				query = query.Where(hp => hp.RegisteredAsHostAt >= hostProfileFilter.RegisteredFrom.Value);
			}

			if (hostProfileFilter.RegisteredTo.HasValue)
			{
				query = query.Where(hp => hp.RegisteredAsHostAt <= hostProfileFilter.RegisteredTo.Value);
			}

			if (hostProfileFilter.ReviewedFrom.HasValue)
			{
				query = query.Where(hp => hp.ReviewedAt.HasValue && hp.ReviewedAt.Value >= hostProfileFilter.ReviewedFrom.Value);
			}

			if (hostProfileFilter.ReviewedTo.HasValue)
			{
				query = query.Where(hp => hp.ReviewedAt.HasValue && hp.ReviewedAt.Value <= hostProfileFilter.ReviewedTo.Value);
			}

			if (hostProfileFilter.MinAverageRating.HasValue)
			{
				query = query.Where(hp => hp.AverageRating >= hostProfileFilter.MinAverageRating.Value);
			}
			if (hostProfileFilter.MaxAverageRating.HasValue)
			{
				query = query.Where(hp => hp.AverageRating <= hostProfileFilter.MaxAverageRating.Value);
			}
			if (hostProfileFilter.MinTotalBookings.HasValue)
			{
				query = query.Where(hp => hp.TotalBookings >= hostProfileFilter.MinTotalBookings.Value);
			}
			if (hostProfileFilter.MaxTotalBookings.HasValue)
			{
				query = query.Where(hp => hp.TotalBookings <= hostProfileFilter.MaxTotalBookings.Value);
			}
			if (hostProfileFilter.MinTotalHomestays.HasValue)
			{
				query = query.Where(hp => hp.TotalHomestays >= hostProfileFilter.MinTotalHomestays.Value);
			}
			if (hostProfileFilter.MaxTotalHomestays.HasValue)
			{
				query = query.Where(hp => hp.TotalHomestays <= hostProfileFilter.MaxTotalHomestays.Value);
			}
			if (hostProfileFilter.MinResponseRate.HasValue)
			{
				query = query.Where(hp => hp.ResponseRate >= hostProfileFilter.MinResponseRate.Value);
			}
			if (hostProfileFilter.MaxResponseRate.HasValue)
			{
				query = query.Where(hp => hp.ResponseRate <= hostProfileFilter.MaxResponseRate.Value);
			}
			// Apply sorting (default by RegisteredAsHostAt desc)

			query = hostProfileFilter.SortBy?.ToLower() switch
			{
				"businessname" => hostProfileFilter.SortDirection == "desc" ? query.OrderByDescending(hp => hp.BusinessName) : query.OrderBy(hp => hp.BusinessName),
				"averageRating" => hostProfileFilter.SortDirection == "desc" ? query.OrderByDescending(hp => hp.AverageRating) : query.OrderBy(hp => hp.AverageRating),
				"totalbookings" => hostProfileFilter.SortDirection == "desc" ? query.OrderByDescending(hp => hp.TotalBookings) : query.OrderBy(hp => hp.TotalBookings),
				"totalhomestays" => hostProfileFilter.SortDirection == "desc" ? query.OrderByDescending(hp => hp.TotalHomestays) : query.OrderBy(hp => hp.TotalHomestays),
				"responsrate" => hostProfileFilter.SortDirection == "desc" ? query.OrderByDescending(hp => hp.ResponseRate) : query.OrderBy(hp => hp.ResponseRate),
				"registeredashostat" => hostProfileFilter.SortDirection == "desc" ? query.OrderByDescending(hp => hp.RegisteredAsHostAt) : query.OrderBy(hp => hp.RegisteredAsHostAt),
				_ => hostProfileFilter.SortDirection == "desc" ? query.OrderByDescending(hp => hp.RegisteredAsHostAt) : query.OrderBy(hp => hp.RegisteredAsHostAt),
			};

			// Get total count before pagination
			var totalCount = await query.CountAsync();
			// Apply pagination
			var items = await query
				.Skip((hostProfileFilter.PageNumber - 1) * hostProfileFilter.PageSize)
				.Take(hostProfileFilter.PageSize)
				.ToListAsync();
			return new PagedResult<HostProfile>(items, totalCount, hostProfileFilter.PageNumber, hostProfileFilter.PageSize);
		}

		public async Task<int> CountApprovedHostsAsync()
		{
			return await _dbSet
				.CountAsync(h => h.Status == HostStatus.Approved);
		}

		public async Task<int> CountActiveHostsAsync(DateTime since)
		{
			return await _dbSet
				.Include(h => h.User)
				.CountAsync(h => h.Status == HostStatus.Approved &&
								 h.IsActive &&
								 h.User.LastLoginAt >= since);
		}

		public async Task<int> CountHostsByRegisteredDateAsync(DateTime startDate, DateTime endDate)
		{
			return await _dbSet
				.CountAsync(h => h.Status == HostStatus.Approved &&
								 h.RegisteredAsHostAt >= startDate &&
								 h.RegisteredAsHostAt < endDate);
		}
	}
}
