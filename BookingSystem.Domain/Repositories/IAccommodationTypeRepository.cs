using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Base;
namespace BookingSystem.Domain.Repositories
{
    public interface IAccommodationTypeRepository : IRepository<AccommodationType>
	{
		Task<bool> IsNameExistsAsync(string name);
		Task<bool> IsNameExistsAsync(string name, Guid id);
		Task<AccommodationType?> GetByNameAsync(string name);
		Task<IEnumerable<AccommodationType>> GetAllActiveAsync();
		Task<IEnumerable<AccommodationType>> GetAllInactiveAsync();
		Task ActivateAsync(Guid id);
		Task DeactivateAsync(Guid id);
	}
}
