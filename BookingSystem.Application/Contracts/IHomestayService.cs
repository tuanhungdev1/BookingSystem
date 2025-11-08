
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.HomestayImageDTO;
using BookingSystem.Application.DTOs.HomestayRuleDTO;
using BookingSystem.Application.DTOs.AvailabilityCalendarDTO;
using BookingSystem.Application.DTOs.HomestayDTO;
using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.RuleDTO;

namespace BookingSystem.Application.Contracts
{
	public interface IHomestayService
	{
		// Basic CRUD
		Task<HomestayDto?> GetByIdAsync(int id);
		Task<HomestayDto> GetHomestayBySlugAsync(string slug);
		Task<PagedResult<HomestayDto>> GetHomestaysByOwnerIdAsync(int ownerId, HomestayFilter filter);
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
		Task<bool> UpdateHomestayAmenitiesAsync(int homestayId, int ownerId, UpdateHomestayAmenitiesDto updateDto);

		// Rules Management
		Task<bool> UpdateHomestayRulesAsync(int homestayId, int ownerId, UpdateHomestayRulesDto updateDto);

		Task<bool> UpdateAvailabilityCalendarsAsync(int homestayId, int ownerId, UpdateHomestayAvailabilityCalendarsDto request);

		// Thêm vào interface IHomestayService
		Task<HomestayDto?> ApproveHomestayAsync(int homestayId, int adminId, ApproveHomestayDto request);
		Task<HomestayDto?> RejectHomestayAsync(int homestayId, int adminId, string rejectionReason);
		Task<IEnumerable<HomestayDto>> GetPendingApprovalsAsync();
		Task<int> CountPendingApprovalsAsync();
		Task<bool> SetFeaturedStatusAsync(int homestayId, int adminId, bool isFeatured);
	}
}
