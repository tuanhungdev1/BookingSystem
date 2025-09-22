using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Repositories
{
	public interface IAccommodationRepository : IRepository<Accommodation>
	{
		Task<PagedResult<Accommodation>> GetPagedAsync(AccommodationFilter filter);
		Task<IEnumerable<Accommodation>> SearchAsync(string searchTerm);
		Task<IEnumerable<Accommodation>> GetByLocationAsync(string city, string country);
		Task<IEnumerable<Accommodation>> GetByTypeAsync(AccommodationType type);
		Task<Accommodation?> GetWithDetailsAsync(Guid id);
		Task<bool> IsNameExistsAsync(string name, Guid? excludeId = null);
	}
}
