using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Domain.Repositories
{
	public interface IPropertyTypeRepository : IRepository<PropertyType>
	{
		Task<PagedResult<PropertyType>> GetPagedResultAsync(PropertyTypeFilter propertyTypeFilter);
	}
}
