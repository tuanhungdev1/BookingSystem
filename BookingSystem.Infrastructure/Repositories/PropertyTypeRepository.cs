using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Infrastructure.Repositories
{
    public class PropertyTypeRepository : Repository<PropertyType>, IPropertyTypeRepository
	{
		public PropertyTypeRepository(BookingDbContext context) : base(context)
		{
		}

		public async Task<PagedResult<PropertyType>> GetPagedResultAsync(PropertyTypeFilter propertyTypeFilter)
		{
			var query = _dbSet.AsQueryable();
			if (!string.IsNullOrEmpty(propertyTypeFilter.SearchTerm))
			{
				query = query.Where(pt => pt.TypeName.ToLower().Contains(propertyTypeFilter.SearchTerm.ToLower()) ||
											pt.Description.ToLower().Contains(propertyTypeFilter.SearchTerm.ToLower())
				);
			}

			if (propertyTypeFilter.IsActive.HasValue)
			{
				query = query.Where(pt => pt.IsActive == propertyTypeFilter.IsActive.Value);
			}

			// Thực hiện sắp xếp theo từ khóa SortBy và SortDirection
			if (!string.IsNullOrEmpty(propertyTypeFilter.SortBy))
			{
				bool isAscending = propertyTypeFilter.SortDirection?.ToLower() != "desc";
				query = propertyTypeFilter.SortBy.ToLower() switch
				{
					"name" => isAscending ? query.OrderBy(pt => pt.TypeName) : query.OrderByDescending(pt => pt.TypeName),
					"createdat" => isAscending ? query.OrderBy(pt => pt.CreatedAt) : query.OrderByDescending(pt => pt.CreatedAt),
					_ => query
				};
			}

			// Áp dụng phân trang
			var totalItems = await query.CountAsync();

			var items = await query
				.Skip((propertyTypeFilter.PageNumber - 1) * propertyTypeFilter.PageSize)
				.Take(propertyTypeFilter.PageSize)
				.ToListAsync();

			// Dùng contructor của PagedResult để trả về kết quả phân trang


			return new PagedResult<PropertyType>(items, totalItems, propertyTypeFilter.PageNumber, propertyTypeFilter.PageSize);
		}
	}
}
