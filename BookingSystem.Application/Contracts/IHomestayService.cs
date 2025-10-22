
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AccommodationDTO.BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.HomestayImageDTO;
using BookingSystem.Application.DTOs.HomestayAmenityDTO;
using BookingSystem.Application.DTOs.HomestayRuleDTO;

namespace BookingSystem.Application.Contracts
{
	public interface IHomestayService
	{
		// Basic CRUD
		Task<HomestayDto?> GetByIdAsync(int id);
		Task<PagedResult<HomestayDto>> GetAllHomestayAsync(HomestayFilter filter);
		Task<HomestayDto?> CreateAsync(int ownerId, CreateHomestayDto request);
		Task<HomestayDto?> UpdateAsync(int homestayId, int ownerId, UpdateHomestayDto request);
		Task<bool> DeleteAsync(int id);

		// Status Management
		Task<bool> ActivateAsync(int homestayId, int userActiveId);
		Task<bool> DeactivateAsync(int homestayId, int userActiveId);

		// Image Management
		Task<bool> UpdateHomestayImages(int homestayId, int ownerId, UpdateHomestayImagesDto updateHomestayImages);

		// Amenities Management
		Task<bool> UpdateHomestayAmenitiesAsync(int homestayId, int ownerId, List<CreateHomestayAmenityDto> amenities);

		// Rules Management
		Task<bool> UpdateHomestayRulesAsync(int homestayId, int ownerId, List<CreateHomestayRuleDto> rules);
	}
}
