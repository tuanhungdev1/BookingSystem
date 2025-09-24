using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AccommodationTypeDTO;
using BookingSystem.Domain.Base;
using BookingSystem.Domain.Repositories;
using Microsoft.Extensions.Logging;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Application.DTOs.ImageDTO;
using BookingSystem.Infrastructure.Repositories;

namespace BookingSystem.Application.Services
{
	public class AccommodationTypeService : IAccommodationTypeService
	{
		private readonly IAccommodationTypeRepository _accommodationTypeRepository;
		private readonly ILogger<AccommodationTypeService> _logger;
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICloudinaryService _cloudinaryService;
		public AccommodationTypeService(
				IAccommodationTypeRepository accommodationTypeRepository,
				ILogger<AccommodationTypeService> logger,
				IMapper mapper,
				IUnitOfWork unitOfWork,
				ICloudinaryService cloudinaryService
			)
		{
			_accommodationTypeRepository = accommodationTypeRepository;
			_logger = logger;
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_cloudinaryService = cloudinaryService;
		}

		public async Task<AccommodationTypeDto?> CreateAsync(CreateAccommodationTypeDto dto)
		{
			if (await _accommodationTypeRepository.IsNameExistsAsync(dto.Name))
			{
				_logger.LogWarning("Attempt to create AccommodationType with duplicate name: {Name}", dto.Name);
				throw new BadRequestException($"AccommodationType with name '{dto.Name}' already exists");
			}

			await _unitOfWork.BeginTransactionAsync();
			string? uploadedPublicId = null;

			try
			{
				var entity = _mapper.Map<AccommodationType>(dto);
				entity.CreatedAt = DateTime.UtcNow;

				// Upload image if provided
				if (dto.Image != null)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(new ImageUploadDto
					{
						File = dto.Image,
						Folder = "accommodation-types"
					});

					if (!uploadResult.Success || uploadResult.Data == null || string.IsNullOrEmpty(uploadResult.Data.SecureUrl))
					{
						throw new BadRequestException(uploadResult.ErrorMessage ?? "Image upload failed");
					}

					entity.Image = uploadResult.Data.SecureUrl;
					uploadedPublicId = uploadResult.Data.PublicId;
				}

				await _accommodationTypeRepository.AddAsync(entity);
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("AccommodationType created: {Name} (Id: {Id})", entity.Name, entity.Id);
				return _mapper.Map<AccommodationTypeDto>(entity);
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();

				if (!string.IsNullOrEmpty(uploadedPublicId))
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(uploadedPublicId);
					if (!deleteResult.Success)
					{
						_logger.LogError("Failed to rollback Cloudinary image with PublicId={PublicId}: {Error}", uploadedPublicId, deleteResult.ErrorMessage);
					}
				}

				_logger.LogError(ex, "Error while creating AccommodationType");
				throw;
			}
		}

		public async Task<IEnumerable<AccommodationTypeDto>> GetAllAccommodationTypeAsync()
		{
			var entities = await _accommodationTypeRepository.GetAllAsync();
			return _mapper.Map<IEnumerable<AccommodationTypeDto>>(entities);
		}

		public async Task<AccommodationTypeDto?> GetByIdAsync(Guid id)
		{
			var entity = await _accommodationTypeRepository.GetByIdAsync(id);
			if (entity == null)
			{
				_logger.LogWarning("AccommodationType with Id {Id} not found", id);
				return null;
			}

			return _mapper.Map<AccommodationTypeDto>(entity);
		}

		public async Task<AccommodationTypeDto?> UpdateAsync(Guid id, UpdateAccommodationTypeDto dto)
		{
			var entity = await _accommodationTypeRepository.GetByIdAsync(id);
			if (entity == null)
			{
				_logger.LogWarning("Attempt to update non-existing AccommodationType with Id {Id}", id);
				throw new NotFoundException($"AccommodationType with Id {id} not found");
			}

			if (await _accommodationTypeRepository.IsNameExistsAsync(dto.Name, id))
			{
				_logger.LogWarning("Attempt to update AccommodationType with duplicate name: {Name}", dto.Name);
				throw new BadRequestException($"AccommodationType with name '{dto.Name}' already exists");
			}

			await _unitOfWork.BeginTransactionAsync();
			string? uploadedPublicId = null;
			string? oldPublicId = null;

			try
			{
				// Store old publicId (if any) for deletion after successful commit
				if (!string.IsNullOrEmpty(entity.Image))
				{
					oldPublicId = _cloudinaryService.GetPublicIdFromUrl(entity.Image);
				}

				// Upload new image if provided
				if (dto.Image != null)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(new ImageUploadDto
					{
						File = dto.Image,
						Folder = "accommodation-types"
					});

					if (!uploadResult.Success || uploadResult.Data == null || string.IsNullOrEmpty(uploadResult.Data.SecureUrl))
					{
						throw new BadRequestException(uploadResult.ErrorMessage ?? "Image upload failed");
					}

					uploadedPublicId = uploadResult.Data.PublicId;
					entity.Image = uploadResult.Data.SecureUrl;
				}

				// Map text fields
				_mapper.Map(dto, entity);
				entity.UpdatedAt = DateTime.UtcNow;

				_accommodationTypeRepository.Update(entity);
				await _unitOfWork.CommitTransactionAsync();

				// Delete old image if it exists
				if (!string.IsNullOrEmpty(oldPublicId))
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(oldPublicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete old Cloudinary image with PublicId={PublicId}: {Error}", oldPublicId, deleteResult.ErrorMessage);
					}
				}

				_logger.LogInformation("AccommodationType updated: {Name} (Id: {Id})", entity.Name, entity.Id);
				return _mapper.Map<AccommodationTypeDto>(entity);
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();

				// Rollback new image if uploaded
				if (!string.IsNullOrEmpty(uploadedPublicId))
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(uploadedPublicId);
					if (!deleteResult.Success)
					{
						_logger.LogError("Failed to rollback Cloudinary image with PublicId={PublicId}: {Error}", uploadedPublicId, deleteResult.ErrorMessage);
					}
				}

				_logger.LogError(ex, "Error while updating AccommodationType {Id}", id);
				throw;
			}
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var entity = await _accommodationTypeRepository.GetByIdAsync(id);
			if (entity == null)
			{
				_logger.LogWarning("Attempt to delete non-existing AccommodationType with Id {Id}", id);
				return false;
			}

			await _unitOfWork.BeginTransactionAsync();

			try
			{
				string? publicIdToDelete = null;
				if (!string.IsNullOrEmpty(entity.Image))
				{
					publicIdToDelete = _cloudinaryService.GetPublicIdFromUrl(entity.Image);
				}

				_accommodationTypeRepository.Remove(entity);
				await _unitOfWork.CommitTransactionAsync();

				// Delete image on Cloudinary
				if (!string.IsNullOrEmpty(publicIdToDelete))
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicIdToDelete);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete Cloudinary image with PublicId={PublicId}: {Error}", publicIdToDelete, deleteResult.ErrorMessage);
					}
				}

				_logger.LogInformation("AccommodationType deleted: Id {Id}", id);
				return true;
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();
				_logger.LogError(ex, "Error while deleting AccommodationType {Id}", id);
				throw;
			}
		}

		public async Task<IEnumerable<AccommodationTypeDto>> GetAllActiveAsync()
		{
			var entities = await _accommodationTypeRepository.GetAllActiveAsync();
			return _mapper.Map<IEnumerable<AccommodationTypeDto>>(entities);
		}

		public async Task<IEnumerable<AccommodationTypeDto>> GetAllInactiveAsync()
		{
			var entities = await _accommodationTypeRepository.GetAllInactiveAsync();
			return _mapper.Map<IEnumerable<AccommodationTypeDto>>(entities);
		}

		public async Task<bool> ActivateAsync(Guid id)
		{
			var existing = await _accommodationTypeRepository.GetByIdAsync(id);
			if (existing == null) return false;

			try
			{
				await _accommodationTypeRepository.ActivateAsync(id);
				_logger.LogInformation("AccommodationType activated: Id {Id}", id);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error activating AccommodationType {Id}", id);
				throw;
			}
		}

		public async Task<bool> DeactivateAsync(Guid id)
		{
			var existing = await _accommodationTypeRepository.GetByIdAsync(id);
			if (existing == null) return false;

			try
			{
				await _accommodationTypeRepository.DeactivateAsync(id);
				_logger.LogInformation("AccommodationType deactivated: Id {Id}", id);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deactivating AccommodationType {Id}", id);
				throw;
			}
		}
	}
}