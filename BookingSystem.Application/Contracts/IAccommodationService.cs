using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;

namespace BookingSystem.Application.Contracts
{
	public interface IAccommodationService
	{
		Task<AccommodationDto?> CreateAsync(CreateAccommodationRequest request);
		Task<AccommodationDto?> UpdateAsync(Guid id, UpdateAccommodationRequest request);
		Task<bool> DeleteAsync(Guid id);
		Task<AccommodationDto?> GetByIdAsync(Guid id);
		Task<PagedResult<AccommodationDto>> GetPagedAsync(AccommodationFilter filter);
		Task<IEnumerable<AccommodationDto>> SearchAsync(string searchTerm);
		Task<IEnumerable<AccommodationDto>> GetByLocationAsync(string city, string country);
		Task<IEnumerable<AccommodationDto>> GetByTypeAsync(Domain.Enums.AccommodationType type);
		Task<bool> ActivateAsync(Guid id);
		Task<bool> DeactivateAsync(Guid id);
	}
}
