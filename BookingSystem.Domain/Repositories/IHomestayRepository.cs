using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Repositories
{
	public interface IHomestayRepository : IRepository<Homestay>
	{
		Task<PagedResult<Homestay>> GetAllHomestayAsync(HomestayFilter filter);
		Task<IEnumerable<Homestay>> SearchAsync(string searchTerm);
		Task<IEnumerable<Homestay>> GetByLocationAsync(string city, string country);
		Task<IEnumerable<Homestay>> GetByTypeAsync(int AccommodationTypeId);
		Task<Homestay?> GetWithDetailsAsync(int id);
		Task<Homestay?> GetHomestayByIdAsync(int id);
		Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
		Task<IEnumerable<Homestay>> GetPendingApprovalsAsync();
		Task<int> CountPendingApprovalsAsync();
		Task<Homestay?> GetHomestayBySlugAsync(string slug);
		Task<IEnumerable<Homestay>> GetHomestaysByHostIdAsync(int hostId);
		Task<IEnumerable<Homestay>> GetHomestaysByOwnerIdAsync(int ownerId);
	}
}
