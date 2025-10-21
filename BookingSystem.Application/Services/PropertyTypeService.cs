using BookingSystem.Application.Contracts;
using BookingSystem.Domain.Repositories;
using BookingSystem.Domain.Base;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Application.DTOs.PropertyTypeDTO;
using BookingSystem.Application.DTOs.ImageDTO;
using BookingSystem.Application.Models.Constants;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Application.Services
{
    public class PropertyTypeService : IPropertyTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IPropertyTypeRepository _propertyTypeRepository;
		private readonly ILogger<PropertyTypeService> _logger;
		private readonly ICloudinaryService _cloudinaryService;

		public PropertyTypeService(
				IUnitOfWork unitOfWork,
				IMapper mapper,
				IPropertyTypeRepository propertyTypeRepository,
				ILogger<PropertyTypeService> logger,
				ICloudinaryService cloudinaryService
			)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_propertyTypeRepository = propertyTypeRepository;
			_logger = logger;
			_cloudinaryService = cloudinaryService;
		}

		public async Task<PagedResult<PropertyTypeDto>> GetPagedResultAsync(PropertyTypeFilter propertyTypeFilter)
		{
			var propertyTypes = await _propertyTypeRepository.GetPagedResultAsync(propertyTypeFilter);
			var propertyTypeDtos = _mapper.Map<List<PropertyTypeDto>>(propertyTypes.Items);
			_logger.LogInformation("Retrieved {Count} property types.", propertyTypeDtos.Count);
			return new PagedResult<PropertyTypeDto>
			{
				Items = propertyTypeDtos,
				TotalCount = propertyTypes.TotalCount,
				PageNumber = propertyTypes.PageNumber,
				PageSize = propertyTypes.PageSize,
				TotalPages = propertyTypes.TotalPages
			};
		}

		public async Task<PropertyTypeDto?> GetByIdAsync(int id)
		{
			var propertyType = await _propertyTypeRepository.GetByIdAsync(id);
			if (propertyType == null)
			{
				_logger.LogWarning("Property type with ID {PropertyTypeId} not found.", id);
				throw new NotFoundException($"Property type with ID {id} not found.");
			}

			_logger.LogInformation("Retrieved property type with ID {PropertyTypeId}.", id);
			return _mapper.Map<PropertyTypeDto>(propertyType);
		}

		public async Task<PropertyTypeDto> CreateAsync(CreatePropertyTypeDto request)
		{
			var uploadedPublicId = string.Empty;
			var newPropertyType = _mapper.Map<Domain.Entities.PropertyType>(request);

			try
			{
				// Bước 1: Thêm mới entity
				await _propertyTypeRepository.AddAsync(newPropertyType);
				await _propertyTypeRepository.SaveChangesAsync(); // ✅ để DB sinh Id

				// Bước 2: Nếu có upload ảnh thì upload
				if (request.IconFile != null)
				{
					var imageUploadDto = new ImageUploadDto
					{
						File = request.IconFile,
						Folder = $"{FolderImages.Properties}/{newPropertyType.Id}",
					};

					var uploadResult = await _cloudinaryService.UploadImageAsync(imageUploadDto);
					if (!uploadResult.Success)
					{
						_logger.LogError("Failed to upload image to Cloudinary: {ErrorMessage}", uploadResult.ErrorMessage);
						throw new BadRequestException("Failed to upload image to Cloudinary");
					}

					newPropertyType.IconUrl = uploadResult.Data.Url;
					uploadedPublicId = uploadResult.Data.PublicId;

					_logger.LogInformation("Successfully uploaded image to Cloudinary for PropertyType: {PublicId}", uploadResult.Data.PublicId);

					// ✅ Cập nhật lại iconUrl
					_propertyTypeRepository.Update(newPropertyType);
					await _propertyTypeRepository.SaveChangesAsync(); // Chỉ cần Save lại, không cần Update
				}

				_logger.LogInformation("Successfully created property type with ID {PropertyTypeId}.", newPropertyType.Id);

				return _mapper.Map<PropertyTypeDto>(newPropertyType);
			}
			catch (Exception ex)
			{
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


		public async Task<PropertyTypeDto?> UpdateAsync(int id, UpdatePropertyTypeDto request)
		{
			var existingPropertyType = await _propertyTypeRepository.GetByIdAsync(id);
			if (existingPropertyType == null)
			{
				_logger.LogWarning("Property type with ID {PropertyTypeId} not found.", id);
				throw new NotFoundException($"Property type with ID {id} not found.");
			}

			var currentImageUrl = existingPropertyType.IconUrl;
			var oldIconPublicId = String.Empty;
			var newIconPublicId = String.Empty;
			try
			{
				// Cập nhật các trường thông tin
				_mapper.Map(request, existingPropertyType);
				existingPropertyType.IconUrl = currentImageUrl;

				if (request.IconFile != null)
				{
					// Lưu PUBLIC ID của hình ảnh cũ để xóa sau khi upload thành công hình mới
					if (!string.IsNullOrEmpty(existingPropertyType.IconUrl))
					{
						oldIconPublicId = _cloudinaryService.GetPublicIdFromUrl(existingPropertyType.IconUrl);
					}
					// Tạo Object ImageUploadDto
					var imageUploadDto = new ImageUploadDto
					{
						File = request.IconFile,
						Folder = $"{FolderImages.Properties}/{existingPropertyType.TypeName}_{existingPropertyType.Id}",
					};
					var uploadResult = await _cloudinaryService.UploadImageAsync(imageUploadDto);
					if (!uploadResult.Success)
					{
						_logger.LogError("Failed to upload image to Cloudinary: {ErrorMessage}", uploadResult.ErrorMessage);
						throw new BadRequestException("Failed to upload image to Cloudinary");
					}
					existingPropertyType.IconUrl = uploadResult.Data.Url;
					newIconPublicId = uploadResult.Data.PublicId;
				}

				// Kiểm tra trường hợp nếu không có hình ảnh được update thì kiểm tra Image Action
				else if (request.ImageAction == ImageAction.Remove && !string.IsNullOrEmpty(existingPropertyType.IconUrl))
				{
					oldIconPublicId = _cloudinaryService.GetPublicIdFromUrl(existingPropertyType.IconUrl);
					existingPropertyType.IconUrl = null;

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
				_propertyTypeRepository.Update(existingPropertyType);
				await _propertyTypeRepository.SaveChangesAsync();
				_logger.LogInformation("Successfully updated property type with ID {PropertyTypeId}.", id);
				return _mapper.Map<PropertyTypeDto>(existingPropertyType);
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
			var existingPropertyType = await _propertyTypeRepository.GetByIdAsync(id);

			if (existingPropertyType == null)
			{
				_logger.LogWarning("Property type with ID {PropertyTypeId} not found.", id);
				throw new NotFoundException($"Property type with ID {id} not found.");
			}

			try
			{
				// Kiểm tra hình ảnh có tồn tại không và xóa nếu cần thiết
				if (!string.IsNullOrEmpty(existingPropertyType.IconUrl))
				{
					var publicId = _cloudinaryService.GetPublicIdFromUrl(existingPropertyType.IconUrl);
					var result = await _cloudinaryService.DeleteImageAsync(publicId);

					if (!result.Success)
					{
						_logger.LogError("Failed to delete image from Cloudinary for PropertyType ID {PropertyTypeId}: {ErrorMessage}", id, result.ErrorMessage);
						throw new BadRequestException("Failed to delete image from Cloudinary");
					}

					_logger.LogInformation("Successfully deleted image from Cloudinary for PropertyType ID {PropertyTypeId}", id);

				}
				// Xóa entity PropertyType
				_propertyTypeRepository.Remove(existingPropertyType);
				await _propertyTypeRepository.SaveChangesAsync();

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting property type with ID {PropertyTypeId}.", id);
				throw new Exception("An error occurred while deleting the property type. Please try again later.");
			}
		}

		public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
		{
			var existingPropertyType = await _propertyTypeRepository.GetByIdAsync(id);
			if (existingPropertyType == null)
			{
				_logger.LogWarning("Property type with ID {PropertyTypeId} not found.", id);
				throw new NotFoundException($"Property type with ID {id} not found.");
			}
			existingPropertyType.IsActive = isActive;
			_propertyTypeRepository.Update(existingPropertyType);
			await _propertyTypeRepository.SaveChangesAsync();
			_logger.LogInformation("Set IsActive={IsActive} for property type with ID {PropertyTypeId}.", isActive, id);
			return true;
		}

		public async Task<bool> ExistsAsync(int id)
		{
			// Kiểm tra sự tồn tại của PropertyType theo ID
			var exists = await _propertyTypeRepository.GetByIdAsync(id);
			return exists != null;
		}


	}
}
