using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Application.DTOs.AmenityDTO;

namespace BookingSystem.Application.Contracts
{
    public interface IAmenityService
    {
		Task<PagedResult<AmenityDto>> GetPagedResultAsync(AmenityFilter amenityFilter);
		Task<AmenityDto?> GetByIdAsync(int id);
		Task<AmenityDto> CreateAsync(CreateAmenityDto request);
		Task<AmenityDto?> UpdateAsync(int id, UpdateAmenityDto request);
		Task DeleteAsync(int id);
		Task<bool> SetActiveStatusAsync(int id, bool isActive);
		Task<bool> ExistsAsync(int id);
	}
}
