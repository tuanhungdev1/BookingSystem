using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Domain.Repositories
{
	public interface IUserRepository: IRepository<User>
	{
		Task<User?> GetByRefreshTokenAsync(string refreshToken);
		Task<PagedResult<User>> GetPagedAsync(UserFilter filter);

		Task<int> CountActiveUsersAsync(DateTime since);
		Task<int> CountUsersByCreatedDateAsync(DateTime startDate, DateTime endDate);
	}
}
