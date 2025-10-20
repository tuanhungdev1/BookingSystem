using AutoMapper;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.AmenityDTO;
using BookingSystem.Application.DTOs.ImageDTO;
using BookingSystem.Application.Models.Constants;

using BookingSystem.Domain.Base;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Services
{
	public class AmenityService : IAmenityService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAmenityRepository _amenityRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<AmenityService> _logger;
		private readonly ICloudinaryService _cloudinaryService;

		public AmenityService(
			IUnitOfWork unitOfWork,
			IAmenityRepository amenityRepository,
			IMapper mapper,
			ILogger<AmenityService> logger,
			ICloudinaryService cloudinaryService)
		{
			_unitOfWork = unitOfWork;
			_amenityRepository = amenityRepository;
			_mapper = mapper;
			_logger = logger;
			_amenityRepository = amenityRepository;
			_cloudinaryService = cloudinaryService;
		}

		public async Task<PagedResult<AmenityDto>> GetPagedResultAsync(AmenityFilter amenityFilter)
		{
			var amenities = await _amenityRepository.GetPagedAsync(amenityFilter);
			var amenityDtos = _mapper.Map<List<AmenityDto>>(amenities.Items);

			_logger.LogInformation("Retrieved {Count} amenities.", amenityDtos.Count);

			return new PagedResult<AmenityDto>
			{
				Items = amenityDtos,
				TotalCount = amenities.TotalCount,
				PageNumber = amenities.PageNumber,
				PageSize = amenities.PageSize,
				TotalPages = amenities.TotalPages
			};
		}

		public async Task<AmenityDto?> GetByIdAsync(int id)
		{
			var amenity = await _amenityRepository.GetByIdAsync(id);
			if (amenity == null)
			{
				_logger.LogWarning("Amenity with ID {AmenityId} not found.", id);
				throw new NotFoundException($"Amenity with ID {id} not found.");
			}

			_logger.LogInformation("Retrieved amenity with ID {AmenityId}.", id);
			return _mapper.Map<AmenityDto>(amenity);
		}

		public async Task<AmenityDto> CreateAsync(CreateAmenityDto request)
		{
			// Tạo biến để lưu PUBLIC ID của hình ảnh đã upload
			var uploadedPublicId = String.Empty;
			var newAmenity = _mapper.Map<Amenity>(request);
			try
			{
				// Thêm entity mới vào repository để lấy ID
				await _amenityRepository.AddAsync(newAmenity);

				// Kiểm tra có tạo hình ảnh hay không
				if (request.IconFile != null)
				{
					// Tạo Object ImageUploadDto
					var imageUploadDto = new ImageUploadDto
					{
						File = request.IconFile,
						Folder = $"{FolderImages.Properties}/{newAmenity.Id}",
					};
					var uploadResult = await _cloudinaryService.UploadImageAsync(imageUploadDto);
					if (!uploadResult.Success)
					{
						_logger.LogError("Failed to upload image to Cloudinary: {ErrorMessage}", uploadResult.ErrorMessage);
						throw new BadRequestException("Failed to upload image to Cloudinary");
					}
					newAmenity.IconUrl = uploadResult.Data.Url;
					uploadedPublicId = uploadResult.Data.PublicId;
					_logger.LogInformation("Successfully uploaded image to Cloudinary for Amenity: {PublicId}", uploadResult.Data.PublicId);
				}

				// Lưu thay đổi vào database
				await _amenityRepository.SaveChangesAsync();
				_logger.LogInformation("Successfully created Amenity type with ID {PropertyTypeId}.", newAmenity.Id);

				// Trả về DTO
				return _mapper.Map<AmenityDto>(newAmenity);
			}
			catch (Exception ex)
			{

				// Nếu có lỗi xảy ra và đã upload hình ảnh, xóa hình ảnh đó khỏi Cloudinary để tránh rác
				if (!string.IsNullOrEmpty(uploadedPublicId))
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(uploadedPublicId);
					if (deleteResult.Success)
					{
						_logger.LogInformation("Rolled back uploaded image from Cloudinary: {PublicId}", uploadedPublicId);
					}
					else
					{
						_logger.LogError("Failed to roll back uploaded image from Cloudinary: {PublicId}, Error: {ErrorMessage}", uploadedPublicId, deleteResult.ErrorMessage);
					}
				}
				_logger.LogError(ex, "Error occurred while creating property type.");
				throw;
			}
		}

		public async Task<AmenityDto?> UpdateAsync(int id, UpdateAmenityDto request)
		{
			var existingAmentity = await _amenityRepository.GetByIdAsync(id);
			if (existingAmentity == null)
			{
				_logger.LogWarning("Amenity with ID {PropertyTypeId} not found.", id);
				throw new NotFoundException($"Amenity type with ID {id} not found.");
			}
			var currentIconUrl = existingAmentity.IconUrl;
			var oldIconPublicId = String.Empty;
			var newIconPublicId = String.Empty;
			try
			{
				// Cập nhật các trường thông tin
				_mapper.Map(request, existingAmentity);
				existingAmentity.IconUrl = currentIconUrl;

				if (request.IconFile != null)
				{
					// Lưu PUBLIC ID của hình ảnh cũ để xóa sau khi upload thành công hình mới
					if (!string.IsNullOrEmpty(existingAmentity.IconUrl))
					{
						oldIconPublicId = _cloudinaryService.GetPublicIdFromUrl(existingAmentity.IconUrl);
					}
					// Tạo Object ImageUploadDto
					var imageUploadDto = new ImageUploadDto
					{
						File = request.IconFile,
						Folder = $"{FolderImages.Properties}/{existingAmentity.Id}",
					};
					var uploadResult = await _cloudinaryService.UploadImageAsync(imageUploadDto);
					if (!uploadResult.Success)
					{
						_logger.LogError("Failed to upload image to Cloudinary: {ErrorMessage}", uploadResult.ErrorMessage);
						throw new BadRequestException("Failed to upload image to Cloudinary");
					}
					existingAmentity.IconUrl = uploadResult.Data.Url;
					newIconPublicId = uploadResult.Data.PublicId;
				}

				// Kiểm tra trường hợp nếu không có hình ảnh được update thì kiểm tra Image Action
				else if (request.IconFile != null
					 && request.ImageAction == ImageAction.Remove
					 && !string.IsNullOrEmpty(existingAmentity.IconUrl))

				{
					oldIconPublicId = _cloudinaryService.GetPublicIdFromUrl(existingAmentity.IconUrl);
					existingAmentity.IconUrl = null;

					// Xóa hình ảnh khỏi Cloudinary
					var deleteResult = await _cloudinaryService.DeleteImageAsync(oldIconPublicId);

					if (!deleteResult.Success)
					{
						_logger.LogError("Failed to delete image from Cloudinary for PropertyType ID {PropertyTypeId}: {ErrorMessage}", id, deleteResult.ErrorMessage);
						throw new BadRequestException("Failed to delete image from Cloudinary");
					}

					_logger.LogInformation("Successfully deleted image from Cloudinary for PropertyType ID {PropertyTypeId}", id);
				}

				// Lưu thay đổi vào database
				_amenityRepository.Update(existingAmentity);
				await _amenityRepository.SaveChangesAsync();
				_logger.LogInformation("Successfully updated property type with ID {PropertyTypeId}.", id);
				return _mapper.Map<AmenityDto>(existingAmentity);
			}
			catch (Exception ex)
			{

				// Nếu có lỗi xảy ra và đã upload hình ảnh mới, xóa hình ảnh đó khỏi Cloudinary để tránh rác
				if (!string.IsNullOrEmpty(newIconPublicId))
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(newIconPublicId);
					if (deleteResult.Success)
					{
						_logger.LogInformation("Rolled back newly uploaded image from Cloudinary: {PublicId}", newIconPublicId);
					}
					else
					{
						_logger.LogError("Failed to roll back newly uploaded image from Cloudinary: {PublicId}, Error: {ErrorMessage}", newIconPublicId, deleteResult.ErrorMessage);
					}
				}
				_logger.LogError(ex, "Error occurred while updating property type with ID {PropertyTypeId}.", id);
				throw;
			}
		}

		public async Task DeleteAsync(int id)
		{
			var existingAmenity = await _amenityRepository.GetByIdAsync(id);

			if (existingAmenity == null)
			{
				_logger.LogWarning("Amenity with ID {AmenityId} not found.", id);
				throw new NotFoundException($"Amenity with ID {id} not found.");
			}

			try
			{
				// Kiểm tra hình ảnh có tồn tại không và xóa nếu cần thiết
				if (!string.IsNullOrEmpty(existingAmenity.IconUrl))
				{
					var publicId = _cloudinaryService.GetPublicIdFromUrl(existingAmenity.IconUrl);
					var result = await _cloudinaryService.DeleteImageAsync(publicId);

					if (!result.Success)
					{
						_logger.LogError("Failed to delete image from Cloudinary for Amenity ID {AmenityId}: {ErrorMessage}", id, result.ErrorMessage);
						throw new BadRequestException("Failed to delete image from Cloudinary");
					}

					_logger.LogInformation("Successfully deleted image from Cloudinary for Amenity ID {AmenityId}", id);

				}
				// Xóa entity PropertyType
				_amenityRepository.Remove(existingAmenity);
				await _amenityRepository.SaveChangesAsync();

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting property type with ID {PropertyTypeId}.", id);
				throw new Exception("An error occurred while deleting the property type. Please try again later.");
			}
		}

		public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
		{
			var existingAmenity = await _amenityRepository.GetByIdAsync(id);
			if (existingAmenity == null)
			{
				_logger.LogWarning("Amenity with ID {AmenityId} not found.", id);
				throw new NotFoundException($"Amenity with ID {id} not found.");
			}

			existingAmenity.IsActive = isActive;
			_amenityRepository.Update(existingAmenity);
			await _amenityRepository.SaveChangesAsync();

			_logger.LogInformation("Set IsActive={IsActive} for amenity with ID {AmenityId}.", isActive, id);

			return true;
		}

		public async Task<bool> ExistsAsync(int id)
		{
			var exists = await _amenityRepository.GetByIdAsync(id);
			return exists != null;
		}
	}
}
