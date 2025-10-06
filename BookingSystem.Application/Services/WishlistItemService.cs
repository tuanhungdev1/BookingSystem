using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.WishlistDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services
{
	public class WishlistItemService : IWishlistItemService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IWishlistItemRepository _wishlistItemRepository;
		private readonly IHomestayRepository _homestayRepository;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<WishlistItemService> _logger;

		public WishlistItemService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IWishlistItemRepository wishlistItemRepository,
			IHomestayRepository homestayRepository,
			UserManager<User> userManager,
			ILogger<WishlistItemService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_wishlistItemRepository = wishlistItemRepository;
			_homestayRepository = homestayRepository;
			_userManager = userManager;
			_logger = logger;
		}

		public async Task<WishlistItemDto> AddToWishlistAsync(int userId, AddToWishlistDto request)
		{
			_logger.LogInformation("User {UserId} attempting to add homestay {HomestayId} to wishlist.", userId, request.HomestayId);

			// Verify user exists
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				_logger.LogWarning("User with ID {UserId} not found.", userId);
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			// Verify homestay exists
			var homestay = await _homestayRepository.GetByIdAsync(request.HomestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", request.HomestayId);
				throw new NotFoundException($"Homestay with ID {request.HomestayId} not found.");
			}

			// Check if homestay is active and approved
			if (!homestay.IsActive || !homestay.IsApproved)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} is not available to add to wishlist.", request.HomestayId);
				throw new BadRequestException($"Homestay with ID {request.HomestayId} is not available.");
			}

			// Check if item already exists in wishlist
			var existingItem = await _wishlistItemRepository.GetByUserAndHomestayAsync(userId, request.HomestayId);
			if (existingItem != null)
			{
				_logger.LogWarning("Homestay {HomestayId} is already in user {UserId}'s wishlist.", request.HomestayId, userId);
				throw new BadRequestException("This homestay is already in your wishlist.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var wishlistItem = new WishlistItem
				{
					UserId = userId,
					HomestayId = request.HomestayId,
					PersonalNote = request.PersonalNote,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _wishlistItemRepository.AddAsync(wishlistItem);
				await _wishlistItemRepository.SaveChangesAsync();

				// Reload with includes for proper mapping
				var savedItem = await _wishlistItemRepository.GetByUserAndHomestayAsync(userId, request.HomestayId);

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Homestay {HomestayId} successfully added to user {UserId}'s wishlist.", request.HomestayId, userId);

				return _mapper.Map<WishlistItemDto>(savedItem);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while adding homestay {HomestayId} to user {UserId}'s wishlist.", request.HomestayId, userId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> RemoveFromWishlistAsync(int userId, int wishlistItemId)
		{
			_logger.LogInformation("User {UserId} attempting to remove wishlist item {WishlistItemId}.", userId, wishlistItemId);

			var wishlistItem = await _wishlistItemRepository.GetByIdAsync(wishlistItemId);
			if (wishlistItem == null)
			{
				_logger.LogWarning("Wishlist item with ID {WishlistItemId} not found.", wishlistItemId);
				throw new NotFoundException($"Wishlist item with ID {wishlistItemId} not found.");
			}

			// Verify ownership
			if (wishlistItem.UserId != userId)
			{
				_logger.LogWarning("User {UserId} does not have permission to remove wishlist item {WishlistItemId}.", userId, wishlistItemId);
				throw new BadRequestException($"You do not have permission to remove this wishlist item.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				_wishlistItemRepository.Remove(wishlistItem);
				await _wishlistItemRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Wishlist item {WishlistItemId} successfully removed from user {UserId}'s wishlist.", wishlistItemId, userId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while removing wishlist item {WishlistItemId}.", wishlistItemId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<bool> RemoveByHomestayAsync(int userId, int homestayId)
		{
			_logger.LogInformation("User {UserId} attempting to remove homestay {HomestayId} from wishlist.", userId, homestayId);

			var wishlistItem = await _wishlistItemRepository.GetByUserAndHomestayAsync(userId, homestayId);
			if (wishlistItem == null)
			{
				_logger.LogWarning("Homestay {HomestayId} not found in user {UserId}'s wishlist.", homestayId, userId);
				throw new NotFoundException($"Homestay with ID {homestayId} is not in your wishlist.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				_wishlistItemRepository.Remove(wishlistItem);
				await _wishlistItemRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Homestay {HomestayId} successfully removed from user {UserId}'s wishlist.", homestayId, userId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while removing homestay {HomestayId} from wishlist.", homestayId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<WishlistItemDto?> UpdateWishlistItemAsync(int userId, int wishlistItemId, UpdateWishlistItemDto request)
		{
			_logger.LogInformation("User {UserId} attempting to update wishlist item {WishlistItemId}.", userId, wishlistItemId);

			var wishlistItem = await _wishlistItemRepository.GetByUserAndHomestayAsync(userId, wishlistItemId);
			if (wishlistItem == null)
			{
				_logger.LogWarning("Wishlist item with ID {WishlistItemId} not found.", wishlistItemId);
				throw new NotFoundException($"Wishlist item with ID {wishlistItemId} not found.");
			}

			// Verify ownership
			if (wishlistItem.UserId != userId)
			{
				_logger.LogWarning("User {UserId} does not have permission to update wishlist item {WishlistItemId}.", userId, wishlistItemId);
				throw new BadRequestException($"You do not have permission to update this wishlist item.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				wishlistItem.PersonalNote = request.PersonalNote;
				wishlistItem.UpdatedAt = DateTime.UtcNow;

				_wishlistItemRepository.Update(wishlistItem);
				await _wishlistItemRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Wishlist item {WishlistItemId} successfully updated.", wishlistItemId);

				return _mapper.Map<WishlistItemDto>(wishlistItem);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating wishlist item {WishlistItemId}.", wishlistItemId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<WishlistItemDto?> GetByIdAsync(int userId, int wishlistItemId)
		{
			var wishlistItem = await _wishlistItemRepository.GetByIdAsync(wishlistItemId);
			if (wishlistItem == null)
			{
				_logger.LogWarning("Wishlist item with ID {WishlistItemId} not found.", wishlistItemId);
				throw new NotFoundException($"Wishlist item with ID {wishlistItemId} not found.");
			}

			// Verify ownership
			if (wishlistItem.UserId != userId)
			{
				_logger.LogWarning("User {UserId} does not have permission to view wishlist item {WishlistItemId}.", userId, wishlistItemId);
				throw new BadRequestException($"You do not have permission to view this wishlist item.");
			}

			return _mapper.Map<WishlistItemDto>(wishlistItem);
		}

		public async Task<PagedResult<WishlistItemDto>> GetUserWishlistAsync(int userId, WishlistFilter filter)
		{
			// Verify user exists
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				_logger.LogWarning("User with ID {UserId} not found.", userId);
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var pagedWishlistItems = await _wishlistItemRepository.GetWishlistByUserIdAsync(userId, filter);
			var wishlistDtos = _mapper.Map<List<WishlistItemDto>>(pagedWishlistItems.Items);

			return new PagedResult<WishlistItemDto>
			{
				Items = wishlistDtos,
				TotalCount = pagedWishlistItems.TotalCount,
				PageSize = pagedWishlistItems.PageSize,
				PageNumber = pagedWishlistItems.PageNumber
			};
		}

		public async Task<bool> IsInWishlistAsync(int userId, int homestayId)
		{
			return await _wishlistItemRepository.ExistsAsync(userId, homestayId);
		}

		public async Task<int> GetWishlistCountAsync(int userId)
		{
			return await _wishlistItemRepository.GetWishlistCountByUserIdAsync(userId);
		}

		public async Task<bool> ClearWishlistAsync(int userId)
		{
			_logger.LogInformation("User {UserId} attempting to clear their wishlist.", userId);

			// Verify user exists
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				_logger.LogWarning("User with ID {UserId} not found.", userId);
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var wishlistItems = await _wishlistItemRepository.FindAsync(w => w.UserId == userId);
				if (wishlistItems.Any())
				{
					_wishlistItemRepository.RemoveRange(wishlistItems);
					await _wishlistItemRepository.SaveChangesAsync();
				}

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("User {UserId}'s wishlist successfully cleared.", userId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while clearing user {UserId}'s wishlist.", userId);
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}
	}
}