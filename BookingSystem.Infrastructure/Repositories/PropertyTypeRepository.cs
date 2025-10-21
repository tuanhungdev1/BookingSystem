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
			if (!string.IsNullOrEmpty(propertyTypeFilter.Search))
			{
				query = query.Where(pt => pt.TypeName.ToLower().Contains(propertyTypeFilter.Search.ToLower()) ||
											pt.Description.ToLower().Contains(propertyTypeFilter.Search.ToLower())
				);
			}

			if (propertyTypeFilter.IsActive.HasValue)
			{
				query = query.Where(pt => pt.IsActive == propertyTypeFilter.IsActive.Value);
			}

			// Thực hiện sắp xếp theo từ khóa SortBy và SortDirection
			if (!string.IsNullOrEmpty(propertyTypeFilter.SortBy))
			{
				bool isAscending = propertyTypeFilter.SortBy?.ToLower() != "desc";
				query = propertyTypeFilter.SortBy switch
				{
					"typeName" => isAscending ? query.OrderBy(pt => pt.TypeName) : query.OrderByDescending(pt => pt.TypeName),
					"description" => isAscending ? query.OrderBy(pt => pt.Description) : query.OrderByDescending(pt => pt.Description),
					"displayOrder" => isAscending ? query.OrderBy(pt => pt.DisplayOrder) : query.OrderByDescending(pt => pt.DisplayOrder),
					"updatedAt" => isAscending ? query.OrderBy(pt => pt.UpdatedAt) : query.OrderByDescending(pt => pt.UpdatedAt),
					"createdAt" => isAscending ? query.OrderBy(pt => pt.CreatedAt) : query.OrderByDescending(pt => pt.CreatedAt),
					_ => query.OrderBy(p => p.CreatedAt)
				};
			}

			// Áp dụng phân trang
			var totalItems = await query.CountAsync();
			Console.WriteLine("Total records in DB: " + totalItems);

			var items = await query
				.Skip((propertyTypeFilter.PageNumber - 1) * propertyTypeFilter.PageSize)
				.Take(propertyTypeFilter.PageSize)
				.ToListAsync();

			// Dùng contructor của PagedResult để trả về kết quả phân trang


			return new PagedResult<PropertyType>(items, totalItems, propertyTypeFilter.PageNumber, propertyTypeFilter.PageSize);
		}
	}
}
