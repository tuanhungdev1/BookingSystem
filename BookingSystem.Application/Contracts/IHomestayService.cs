
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AccommodationDTO.BookingSystem.Application.DTOs;

namespace BookingSystem.Application.Contracts
{
	public interface IHomestayService
	{
		Task<HomestayDto?> CreateAsync(int ownerId, CreateHomestayDto request);
		Task<HomestayDto?> UpdateAsync(int id, UpdateHomestayDto request);
		Task<bool> DeleteAsync(int id);
		Task<HomestayDto?> GetByIdAsync(int id);
		Task<PagedResult<HomestayDto>> GetAllHomestayAsync(HomestayFilter filter);
		Task<bool> ActivateAsync(int homestayId, int userActiveId);
		Task<bool> DeactivateAsync(int homestayId, int userActiveId);
	}
}
