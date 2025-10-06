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
	public interface IWishlistItemRepository : IRepository<WishlistItem>
	{
		Task<PagedResult<WishlistItem>> GetWishlistByUserIdAsync(int userId, WishlistFilter filter);
		Task<WishlistItem?> GetByUserAndHomestayAsync(int userId, int homestayId);
		Task<bool> ExistsAsync(int userId, int homestayId);
		Task<int> GetWishlistCountByUserIdAsync(int userId);
		Task<IEnumerable<WishlistItem>> GetWishlistWithDetailsAsync(int userId);
	}
}
