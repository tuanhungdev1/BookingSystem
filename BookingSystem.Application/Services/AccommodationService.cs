using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services
{
	public class AccommodationService : IAccommodationService
	{
		private readonly IAccommodationRepository _accommodationRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ILogger<AccommodationService> _logger;

		public AccommodationService(
			IAccommodationRepository accommodationRepository,
			IUnitOfWork unitOfWork,
			IMapper mapper,
			ILogger<AccommodationService> logger)
		{
			_accommodationRepository = accommodationRepository;
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<AccommodationDto?> CreateAsync(CreateAccommodationRequest request)
		{
			// Kiểm tra tên đã tồn tại
			if (await _accommodationRepository.IsNameExistsAsync(request.Name))
			{
				_logger.LogError("Accommodation creation failed: Name {Name} already exists", request.Name);
				throw new BadRequestException("Accommodation name already exists");
			}

			var accommodation = _mapper.Map<Accommodation>(request);
			accommodation.CreatedAt = DateTime.UtcNow;

			
			try
			{
				await _accommodationRepository.AddAsync(accommodation);
				await _unitOfWork.SaveChangesAsync();

				_logger.LogInformation("Accommodation created successfully: {Name} (ID: {Id})",
					accommodation.Name, accommodation.Id);

				return _mapper.Map<AccommodationDto>(accommodation);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating accommodation: {Name}", request.Name);
				//throw new BadRequestException("Failed to create accommodation");
				throw;
			}
		}

		public async Task<AccommodationDto?> UpdateAsync(Guid id, UpdateAccommodationRequest request)
		{
			var accommodation = await _accommodationRepository.GetByIdAsync(id);
			if (accommodation == null)
			{
				_logger.LogError("Accommodation update failed: Accommodation {Id} not found", id);
				throw new NotFoundException("Accommodation not found");
			}

			// Kiểm tra tên đã tồn tại (trừ chính nó)
			if (await _accommodationRepository.IsNameExistsAsync(request.Name, id))
			{
				_logger.LogError("Accommodation update failed: Name {Name} already exists", request.Name);
				throw new BadRequestException("Accommodation name already exists");
			}

			_mapper.Map(request, accommodation);
			accommodation.UpdatedAt = DateTime.UtcNow;

			try
			{
				_accommodationRepository.Update(accommodation);
				await _unitOfWork.SaveChangesAsync();

				_logger.LogInformation("Accommodation updated successfully: {Name} (ID: {Id})",
					accommodation.Name, accommodation.Id);

				return _mapper.Map<AccommodationDto>(accommodation);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating accommodation: {Id}", id);
				throw new BadRequestException("Failed to update accommodation");
			}
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var accommodation = await _accommodationRepository.GetWithDetailsAsync(id);
			if (accommodation == null)
			{
				_logger.LogError("Accommodation deletion failed: Accommodation {Id} not found", id);
				return false;
			}

			// Kiểm tra có rooms đang được sử dụng không
			if (accommodation.Rooms.Any())
			{
				_logger.LogError("Accommodation deletion failed: Accommodation {Id} has existing rooms", id);
				throw new BadRequestException("Cannot delete accommodation with existing rooms");
			}

			try
			{
				_accommodationRepository.Remove(accommodation);
				await _unitOfWork.SaveChangesAsync();

				_logger.LogInformation("Accommodation deleted successfully: {Name} (ID: {Id})",
					accommodation.Name, id);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting accommodation: {Id}", id);
				throw new BadRequestException("Failed to delete accommodation");
			}
		}

		public async Task<AccommodationDto?> GetByIdAsync(Guid id)
		{
			var accommodation = await _accommodationRepository.GetWithDetailsAsync(id);
			if (accommodation == null)
				return null;

			var dto = _mapper.Map<AccommodationDto>(accommodation);
			dto.RoomTypesCount = accommodation.RoomTypes.Count;
			dto.RoomsCount = accommodation.Rooms.Count;

			return dto;
		}

		public async Task<PagedResult<AccommodationDto>> GetPagedAsync(AccommodationFilter filter)
		{
			var pagedResult = await _accommodationRepository.GetPagedAsync(filter);

			var dtoItems = pagedResult.Items.Select(a => {
				var dto = _mapper.Map<AccommodationDto>(a);
				dto.RoomTypesCount = a.RoomTypes.Count;
				dto.RoomsCount = a.Rooms.Count;
				return dto;
			}).ToList();

			return new PagedResult<AccommodationDto>
			{
				Items = dtoItems,
				TotalCount = pagedResult.TotalCount,
				PageNumber = pagedResult.PageNumber,
				PageSize = pagedResult.PageSize,
				TotalPages = pagedResult.TotalPages
			};
		}

		public async Task<IEnumerable<AccommodationDto>> SearchAsync(string searchTerm)
		{
			var accommodations = await _accommodationRepository.SearchAsync(searchTerm);
			return accommodations.Select(a => _mapper.Map<AccommodationDto>(a));
		}

		public async Task<IEnumerable<AccommodationDto>> GetByLocationAsync(string city, string country)
		{
			var accommodations = await _accommodationRepository.GetByLocationAsync(city, country);
			return accommodations.Select(a => _mapper.Map<AccommodationDto>(a));
		}

		public async Task<IEnumerable<AccommodationDto>> GetByTypeAsync(Guid type)
		{
			var accommodations = await _accommodationRepository.GetByTypeAsync(type);
			return accommodations.Select(a => _mapper.Map<AccommodationDto>(a));
		}

		public async Task<bool> ActivateAsync(Guid id)
		{
			var accommodation = await _accommodationRepository.GetByIdAsync(id);
			if (accommodation == null)
				return false;

			accommodation.IsActive = true;
			accommodation.UpdatedAt = DateTime.UtcNow;

			_accommodationRepository.Update(accommodation);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Accommodation activated: {Name} (ID: {Id})", accommodation.Name, id);
			return true;
		}

		public async Task<bool> DeactivateAsync(Guid id)
		{
			var accommodation = await _accommodationRepository.GetByIdAsync(id);
			if (accommodation == null)
				return false;

			accommodation.IsActive = false;
			accommodation.UpdatedAt = DateTime.UtcNow;

			_accommodationRepository.Update(accommodation);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Accommodation deactivated: {Name} (ID: {Id})", accommodation.Name, id);
			return true;
		}
	}
}
