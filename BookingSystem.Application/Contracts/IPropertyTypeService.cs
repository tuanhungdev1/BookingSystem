using BookingSystem.Application.DTOs.PropertyTypeDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Application.Contracts
{
    public interface IPropertyTypeService
    {
		Task<PagedResult<PropertyTypeDto>> GetPagedResultAsync(PropertyTypeFilter propertyTypeFilter);
		Task<PropertyTypeDto?> GetByIdAsync(int id);
		Task<PropertyTypeDto> CreateAsync(CreatePropertyTypeDto request);
		Task<PropertyTypeDto?> UpdateAsync(int id, UpdatePropertyTypeDto request);
		Task DeleteAsync(int id);
		Task<bool> SetActiveStatusAsync(int id, bool isActive);
		Task<bool> ExistsAsync(int id);

	}
}
