using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Repositories
{
    public interface IAmenityRepository : IRepository<Amenity>
	{
		Task<IEnumerable<Amenity>> GetPopularAmenitiesAsync(int count);
		Task<PagedResult<Amenity>> GetPagedAsync(AmenityFilter amenityFilter);
		Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
		Task<IEnumerable<Amenity>> GetActiveAmenitiesAsync();
		Task<IEnumerable<Amenity>> GetAmenitiesByCategoryAsync(string category);
		Task<IEnumerable<string>> GetAllCategoriesAsync();
		Task<int> GetTotalAmenitiesCountAsync();
		Task<int> GetActiveAmenitiesCountAsync();
	}
}
