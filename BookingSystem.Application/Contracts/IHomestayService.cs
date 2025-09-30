
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Base;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Application.DTOs.AccommodationDTO.BookingSystem.Application.DTOs;

namespace BookingSystem.Application.Contracts
{
	public interface IHomestayService
	{
		Task<HomestayDto?> CreateAsync(CreateHomestayDto request);
		Task<HomestayDto?> UpdateAsync(int id, UpdateHomestayDto request);
		Task<bool> DeleteAsync(int id);
		Task<HomestayDto?> GetByIdAsync(int id);
		Task<PagedResult<HomestayDto>> GetAllHomestayAsync(HomestayFilter filter);
		Task<bool> ActivateAsync(int id);
		Task<bool> DeactivateAsync(int id);
	}
}
