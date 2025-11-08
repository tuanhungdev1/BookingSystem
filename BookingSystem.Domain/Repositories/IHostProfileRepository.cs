using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Domain.Repositories
{
    public interface IHostProfileRepository : IRepository<HostProfile>
	{
        Task<HostProfile?> GetByUserIdAsync(int userId);
		Task<PagedResult<HostProfile>> GetPagedHostProfilesAsync(HostProfileFilter hostProfileFilter);
		Task<int> CountApprovedHostsAsync();
		Task<int> CountActiveHostsAsync(DateTime since);
		Task<int> CountHostsByRegisteredDateAsync(DateTime startDate, DateTime endDate);
	}
}
