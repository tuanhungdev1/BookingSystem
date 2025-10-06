using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.WishlistDTO;
using BookingSystem.Application.Models.Responses;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class WishlistItemController : ControllerBase
	{
		private readonly IWishlistItemService _wishlistItemService;
		private readonly ILogger<WishlistItemController> _logger;

		public WishlistItemController(
			IWishlistItemService wishlistItemService,
			ILogger<WishlistItemController> logger)
		{
			_wishlistItemService = wishlistItemService;
			_logger = logger;
		}

		private int GetCurrentUserId()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdClaim))
				throw new UnauthorizedAccessException("User ID not found in token.");

			return int.Parse(userIdClaim);
		}

		/// <summary>
		/// Add a homestay to the user's wishlist
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<ApiResponse<WishlistItemDto>>> AddToWishlist([FromBody] AddToWishlistDto dto)
		{
			var userId = GetCurrentUserId();
			var result = await _wishlistItemService.AddToWishlistAsync(userId, dto);

			return Ok(new ApiResponse<WishlistItemDto>
			{
				Success = true,
				Message = "Homestay added to wishlist successfully.",
				Data = result
			});
		}

		/// <summary>
		/// Remove a wishlist item by ID
		/// </summary>
		[HttpDelete("{wishlistItemId:int}")]
		public async Task<ActionResult<ApiResponse<object>>> RemoveFromWishlist(int wishlistItemId)
		{
			var userId = GetCurrentUserId();
			var success = await _wishlistItemService.RemoveFromWishlistAsync(userId, wishlistItemId);

			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "Wishlist item removed successfully." : "Failed to remove wishlist item."
			});
		}

		/// <summary>
		/// Remove a homestay from wishlist using its HomestayId
		/// </summary>
		[HttpDelete("by-homestay/{homestayId:int}")]
		public async Task<ActionResult<ApiResponse<object>>> RemoveByHomestay(int homestayId)
		{
			var userId = GetCurrentUserId();
			var success = await _wishlistItemService.RemoveByHomestayAsync(userId, homestayId);

			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "Homestay removed from wishlist." : "Failed to remove homestay."
			});
		}

		/// <summary>
		/// Update personal note in a wishlist item
		/// </summary>
		[HttpPut("{wishlistItemId:int}")]
		public async Task<ActionResult<ApiResponse<WishlistItemDto>>> UpdateWishlistItem(
			int wishlistItemId, [FromBody] UpdateWishlistItemDto dto)
		{
			var userId = GetCurrentUserId();
			var updatedItem = await _wishlistItemService.UpdateWishlistItemAsync(userId, wishlistItemId, dto);

			return Ok(new ApiResponse<WishlistItemDto>
			{
				Success = true,
				Message = "Wishlist item updated successfully.",
				Data = updatedItem
			});
		}

		/// <summary>
		/// Get a specific wishlist item by ID
		/// </summary>
		[HttpGet("{wishlistItemId:int}")]
		public async Task<ActionResult<ApiResponse<WishlistItemDto>>> GetById(int wishlistItemId)
		{
			var userId = GetCurrentUserId();
			var result = await _wishlistItemService.GetByIdAsync(userId, wishlistItemId);

			return Ok(new ApiResponse<WishlistItemDto>
			{
				Success = true,
				Data = result
			});
		}

		/// <summary>
		/// Get all wishlist items for the current user (with paging/filter)
		/// </summary>
		[HttpGet]
		public async Task<ActionResult<ApiResponse<PagedResult<WishlistItemDto>>>> GetUserWishlist([FromQuery] WishlistFilter filter)
		{
			var userId = GetCurrentUserId();
			var result = await _wishlistItemService.GetUserWishlistAsync(userId, filter);

			return Ok(new ApiResponse<PagedResult<WishlistItemDto>>
			{
				Success = true,
				Data = result
			});
		}

		/// <summary>
		/// Check if a homestay is in user's wishlist
		/// </summary>
		[HttpGet("exists/{homestayId:int}")]
		public async Task<ActionResult<ApiResponse<bool>>> IsInWishlist(int homestayId)
		{
			var userId = GetCurrentUserId();
			var exists = await _wishlistItemService.IsInWishlistAsync(userId, homestayId);

			return Ok(new ApiResponse<bool>
			{
				Success = true,
				Data = exists
			});
		}

		/// <summary>
		/// Get wishlist count for the current user
		/// </summary>
		[HttpGet("count")]
		public async Task<ActionResult<ApiResponse<int>>> GetWishlistCount()
		{
			var userId = GetCurrentUserId();
			var count = await _wishlistItemService.GetWishlistCountAsync(userId);

			return Ok(new ApiResponse<int>
			{
				Success = true,
				Data = count
			});
		}

		/// <summary>
		/// Clear all wishlist items for the current user
		/// </summary>
		[HttpDelete("clear")]
		public async Task<ActionResult<ApiResponse<object>>> ClearWishlist()
		{
			var userId = GetCurrentUserId();
			var success = await _wishlistItemService.ClearWishlistAsync(userId);

			return Ok(new ApiResponse<object>
			{
				Success = success,
				Message = success ? "Wishlist cleared successfully." : "Failed to clear wishlist."
			});
		}
	}
}
