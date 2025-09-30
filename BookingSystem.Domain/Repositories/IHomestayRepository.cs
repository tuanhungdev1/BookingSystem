using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Repositories
{
	public interface IHomestayRepository : IRepository<Homestay>
	{
		Task<PagedResult<Homestay>> GetPagedAsync(HomestayFilter filter);
		Task<IEnumerable<Homestay>> SearchAsync(string searchTerm);
		Task<IEnumerable<Homestay>> GetByLocationAsync(string city, string country);
		Task<IEnumerable<Homestay>> GetByTypeAsync(int AccommodationTypeId);
		Task<Homestay?> GetWithDetailsAsync(int id);
		Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
	}
}
