using BookingSystem.Application.DTOs.WishlistDTO;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;

namespace BookingSystem.Application.Contracts
{
	public interface IWishlistItemService
	{
		Task<WishlistItemDto> AddToWishlistAsync(int userId, AddToWishlistDto request);
		Task<bool> RemoveFromWishlistAsync(int userId, int wishlistItemId);
		Task<bool> RemoveByHomestayAsync(int userId, int homestayId);
		Task<WishlistItemDto?> UpdateWishlistItemAsync(int userId, int wishlistItemId, UpdateWishlistItemDto request);
		Task<WishlistItemDto?> GetByIdAsync(int userId, int wishlistItemId);
		Task<PagedResult<WishlistItemDto>> GetUserWishlistAsync(int userId, WishlistFilter filter);
		Task<bool> IsInWishlistAsync(int userId, int homestayId);
		Task<int> GetWishlistCountAsync(int userId);
		Task<bool> ClearWishlistAsync(int userId);
	}
}
