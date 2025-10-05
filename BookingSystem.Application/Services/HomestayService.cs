using BookingSystem.Application.Contracts;
using BookingSystem.Domain.Base;
using AutoMapper;
using BookingSystem.Domain.Repositories;
using Microsoft.Extensions.Logging;
using BookingSystem.Application.DTOs.AccommodationDTO;
using BookingSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using BookingSystem.Domain.Entities;
using BookingSystem.Application.DTOs.ImageDTO;
using BookingSystem.Application.Models.Constants;
using BookingSystem.Domain.Base.Filter;
using BookingSystem.Application.DTOs.AccommodationDTO.BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.HomestayImageDTO;

namespace BookingSystem.Application.Services
{
    public class HomestayService : IHomestayService
    {
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHomestayRepository _homestayRepository;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<HomestayService> _logger;
		private readonly ICloudinaryService _cloudinaryService;
		private readonly IPropertyTypeRepository _propertyTypeRepository;
		private readonly IHomestayImageRepository _homestayImageRepository;
		private readonly IAmenityRepository _amenityRepository;
		private readonly IHomestayAmenityRepository _homestayAmenityRepository;
		private readonly IRuleRepository _ruleRepository;
		private readonly IHomestayRuleRepository _homestayRuleRepository;

		public HomestayService(IUnitOfWork unitOfWork,
								IMapper mapper,
								IHomestayRepository homestayRepository,
								ILogger<HomestayService> logger,
								ICloudinaryService cloudinaryService,
								IPropertyTypeRepository propertyTypeRepository,
								UserManager<User> userManager,
								IHomestayImageRepository homestayImageRepository,
								IAmenityRepository amenityRepository,
								IHomestayAmenityRepository homestayAmenityRepository,
								IRuleRepository ruleRepository,
								IHomestayRuleRepository homestayRuleRepository
		)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_homestayRepository = homestayRepository;
			_logger = logger;
			_cloudinaryService = cloudinaryService;
			_propertyTypeRepository = propertyTypeRepository;
			_userManager = userManager;
			_homestayImageRepository = homestayImageRepository;
			_amenityRepository = amenityRepository;
			_homestayAmenityRepository = homestayAmenityRepository;
			_ruleRepository = ruleRepository;
			_homestayRuleRepository = homestayRuleRepository;
		}

		public async Task<bool> UpdateHomestayImages(int homestayId, int owerId, UpdateHomestayImagesDto updateHomestayImages)
		{
			// check ower exist
			var existingOwner = await _userManager.FindByIdAsync(owerId.ToString());
			if (existingOwner == null)
			{
				_logger.LogWarning("Owner with ID {OwnerId} not found.", owerId);
				throw new NotFoundException($"Owner with ID {owerId} not found.");
			}
			// Check role of user is "Host" and "Admin"
			var roles = await _userManager.GetRolesAsync(existingOwner);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				_logger.LogWarning("User with ID {OwnerId} does not have the 'Host' or 'Admin' role.", owerId);
				throw new BadRequestException($"User with ID {owerId} does not have the 'Host' or 'Admin' role.");
			}
			// if not Admin, check owerId is the homestay's owner
			if (!roles.Contains("Admin"))
			{
				var homestay = await _homestayRepository.GetByIdAsync(homestayId);
				if (homestay == null)
				{
					_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
					throw new NotFoundException($"Homestay with ID {homestayId} not found.");
				}
				if (homestay.OwnerId != existingOwner.Id)
				{
					_logger.LogWarning("User with ID {OwnerId} does not have permission to update this homestay.", owerId);
					throw new BadRequestException($"User with ID {owerId} does not have permission to update this homestay.");
				}
			}
			var homestayToUpdate = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestayToUpdate == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			var uploadedPublicIds = new List<string>();

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var existingImages = await _homestayImageRepository.GetByHomestayIdAsync(homestayId);

				var imagesToDelete = existingImages.Where(img => !updateHomestayImages.KeepImageIds.Contains(img.Id)).ToList();

				// Get public IDs of images to delete for Cloudinary deletion
				var publicIdsToDelete = imagesToDelete
					.Select(img => _cloudinaryService.GetPublicIdFromUrl(img.ImageUrl))
					.ToList();

				// Delete images from Cloudinary

				foreach (var publicId in publicIdsToDelete)
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete image with PublicId {PublicId} from Cloudinary.", publicId);
						throw new BadRequestException($"Failed to delete image with PublicId {publicId} from Cloudinary.");
					}
				}

				// Remove image records from database

				foreach (var image in imagesToDelete)
				{
					_homestayImageRepository.Remove(image);
				}
				await _homestayImageRepository.SaveChangesAsync();

				// Upload new images to Cloudinary and create HomestayImage records

				foreach (var imageDto in updateHomestayImages.NewImages)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(new ImageUploadDto
					{
						File = imageDto.ImageFile,
						Folder = $"{FolderImages.Homestays}/{homestayToUpdate.Id}"
					});
					if (!uploadResult.Success || uploadResult.Data == null)
					{
						_logger.LogError("Image upload failed: {ErrorMessage}", uploadResult.ErrorMessage);
						throw new BadRequestException($"Image upload failed: {uploadResult.ErrorMessage}");
					}
					// Keep track of successfully uploaded images for potential rollback
					uploadedPublicIds.Add(uploadResult.Data.PublicId);
					// Use AutoMapper to map ImageResponseDto to HomestayImage entity
					var homestayImage = _mapper.Map<HomestayImage>(imageDto);
					homestayImage.HomestayId = homestayToUpdate.Id;
					homestayImage.ImageUrl = uploadResult.Data.Url;
					await _homestayImageRepository.AddAsync(homestayImage);
				}
				await _homestayImageRepository.SaveChangesAsync();

				// Update homestay image exists

				foreach (var updateImage in updateHomestayImages.UpdateExistingImages)
				{
					var existingImage = existingImages.FirstOrDefault(img => img.Id == updateImage.ImageId);

					// If image not found, skip to next
					if (existingImage == null) continue;
					// Map updated fields from DTO to entity
					_mapper.Map(updateImage, existingImage);
					_homestayImageRepository.Update(existingImage);
				}
				await _homestayImageRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Homestay images for Homestay ID {HomestayId} updated successfully.", homestayId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during homestay images update. Initiating rollback.");
				// Rollback uploaded images in Cloudinary
				foreach (var publicId in uploadedPublicIds)
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete image with PublicId {PublicId} during rollback.", publicId);
					}
				}
				throw;
			}
		}

		public async Task<HomestayDto?> UpdateAsync(int homestayId, int owerId, UpdateHomestayDto request)
		{
			// check ower exist
			var existingOwner = await _userManager.FindByIdAsync(owerId.ToString());
			if (existingOwner == null)
			{
				_logger.LogWarning("Owner with ID {OwnerId} not found.", owerId);
				throw new NotFoundException($"Owner with ID {owerId} not found.");
			}

			// Check role of user is "Host" and "Admin"
			var roles = await _userManager.GetRolesAsync(existingOwner);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				_logger.LogWarning("User with ID {OwnerId} does not have the 'Host' or 'Admin' role.", owerId);
				throw new BadRequestException($"User with ID {owerId} does not have the 'Host' or 'Admin' role.");
			}

			// if not Admin, check owerId is the homestay's owner
			if (!roles.Contains("Admin"))
			{
				var homestay = await _homestayRepository.GetByIdAsync(homestayId);
				if (homestay == null)
				{
					_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
					throw new NotFoundException($"Homestay with ID {homestayId} not found.");
				}
				if (homestay.OwnerId != existingOwner.Id)
				{
					_logger.LogWarning("User with ID {OwnerId} does not have permission to update this homestay.", owerId);
					throw new BadRequestException($"User with ID {owerId} does not have permission to update this homestay.");
				}
			}

			var homestayToUpdate = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestayToUpdate == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			if (request.PropertyTypeId.HasValue)
			{
				var existingPropertyType = await _propertyTypeRepository.GetByIdAsync(request.PropertyTypeId.Value);
				if (existingPropertyType == null)
				{
					_logger.LogWarning("Property type with ID {PropertyTypeId} not found.", request.PropertyTypeId);
					throw new NotFoundException($"Property type with ID {request.PropertyTypeId} not found.");
				}
				homestayToUpdate.PropertyTypeId = request.PropertyTypeId.Value;
			}

			// Map updated fields from DTO to entity
			_mapper.Map(request, homestayToUpdate);
			homestayToUpdate.UpdatedAt = DateTime.UtcNow;
			homestayToUpdate.UpdatedBy = existingOwner.Id.ToString();
			_homestayRepository.Update(homestayToUpdate);
			await _homestayRepository.SaveChangesAsync();

			return _mapper.Map<HomestayDto>(homestayToUpdate);
		}

		public async Task<PagedResult<HomestayDto>> GetAllHomestayAsync(HomestayFilter filter)
		{
			var pagedHomestays = await _homestayRepository.GetAllHomestayAsync(filter);
			var homestayDtos = _mapper.Map<List<HomestayDto>>(pagedHomestays.Items);
			return new PagedResult<HomestayDto>
			{
				Items = homestayDtos,
				TotalCount = pagedHomestays.TotalCount,
				PageSize = pagedHomestays.PageSize,
				PageNumber = pagedHomestays.PageNumber
			};
		}

		public async Task<bool> DeactivateAsync(int homestayId, int userActiveId)
		{
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}
			if (!homestay.IsApproved)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} is not approved and cannot be activated.", homestayId);
				throw new BadRequestException($"Homestay with ID {homestayId} is not approved and cannot be activated.");
			}

			var user = await _userManager.FindByIdAsync(userActiveId.ToString());
			if (user == null)
			{
				_logger.LogWarning("User with ID {UserId} not found.", userActiveId);
				throw new NotFoundException($"User with ID {userActiveId} not found.");
			}

			// Check role of user is "Admin" or "Host"
			var roles = await _userManager.GetRolesAsync(user);

			// If Admin activate any homestay

			// If Host activate their own homestay
			if (!roles.Contains("Admin") && (!roles.Contains("Host") || homestay.OwnerId != user.Id))
			{
				_logger.LogWarning("User with ID {UserId} does not have permission to activate this homestay.", userActiveId);
				throw new BadRequestException($"User with ID {userActiveId} does not have permission to activate this homestay.");
			}

			homestay.IsActive = false;
			homestay.UpdatedAt = DateTime.UtcNow;
			homestay.UpdatedBy = user.Id.ToString();
			_homestayRepository.Update(homestay);
			await _homestayRepository.SaveChangesAsync();
			_logger.LogInformation("Homestay with ID {HomestayId} activated successfully.", homestayId);
			return true;
		}

		public async Task<bool> ActivateAsync(int homestayId, int userActiveId)
		{
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}
			if (!homestay.IsApproved)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} is not approved and cannot be activated.", homestayId);
				throw new BadRequestException($"Homestay with ID {homestayId} is not approved and cannot be activated.");
			}

			var user = await _userManager.FindByIdAsync(userActiveId.ToString());
			if (user == null)
			{
				_logger.LogWarning("User with ID {UserId} not found.", userActiveId);
				throw new NotFoundException($"User with ID {userActiveId} not found.");
			}

			// Check role of user is "Admin" or "Host"
			var roles = await _userManager.GetRolesAsync(user);

			// If Admin activate any homestay

			// If Host activate their own homestay
			if (!roles.Contains("Admin") && (!roles.Contains("Host") || homestay.OwnerId != user.Id))
			{
				_logger.LogWarning("User with ID {UserId} does not have permission to activate this homestay.", userActiveId);
				throw new BadRequestException($"User with ID {userActiveId} does not have permission to activate this homestay.");
			}

			homestay.IsActive = true;
			homestay.UpdatedAt = DateTime.UtcNow;
			homestay.UpdatedBy = user.Id.ToString(); 
			_homestayRepository.Update(homestay);
			await _homestayRepository.SaveChangesAsync();
			_logger.LogInformation("Homestay with ID {HomestayId} activated successfully.", homestayId);
			return true;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var homestay = await _homestayRepository.GetByIdAsync(id);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", id);
				throw new NotFoundException($"Homestay with ID {id} not found.");
			}
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				// Delete associated images from Cloudinary and database
				var homestayImages = await _homestayImageRepository.GetByHomestayIdAsync(homestay.Id);
				foreach (var image in homestayImages)
				{
					var publicId = _cloudinaryService.GetPublicIdFromUrl(image.ImageUrl);
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete image with PublicId {PublicId} from Cloudinary.", publicId);
						throw new BadRequestException($"Failed to delete image with PublicId {publicId} from Cloudinary.");
					}
					_homestayImageRepository.Remove(image);
				}
				await _homestayImageRepository.SaveChangesAsync();
				// Delete associated amenities
				var homestayAmenities = await _homestayAmenityRepository.GetByHomestayIdAsync(homestay.Id);
				foreach (var amenity in homestayAmenities)
				{
					_homestayAmenityRepository.Remove(amenity);
				}
				await _homestayAmenityRepository.SaveChangesAsync();
				// Delete associated rules
				var homestayRules = await _homestayRuleRepository.GetByHomestayIdAsync(homestay.Id);
				foreach (var rule in homestayRules)
				{
					_homestayRuleRepository.Remove(rule);
				}
				await _homestayRuleRepository.SaveChangesAsync();
				// Finally, delete the homestay
				_homestayRepository.Remove(homestay);
				await _homestayRepository.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Homestay with ID {HomestayId} deleted successfully.", id);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during homestay deletion. Initiating rollback.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<HomestayDto?> GetByIdAsync(int id)
		{
			var homestay = await _homestayRepository.GetByIdAsync(id);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", id);
				throw new NotFoundException($"Homestay with ID {id} not found.");
			}
			return _mapper.Map<HomestayDto>(homestay);
		}

		public async Task<HomestayDto?> CreateAsync(int ownerId, CreateHomestayDto request)
		{
			var uploadedPublicIds = new List<string>();

			_logger.LogInformation("Starting homestay creation process.");

			var existingPropertyType = await _propertyTypeRepository.GetByIdAsync(request.PropertyTypeId);

			if (existingPropertyType == null)
			{
				_logger.LogWarning("Property type with ID {PropertyTypeId} not found.", request.PropertyTypeId);
				throw new NotFoundException($"Property type with ID {request.PropertyTypeId} not found.");
			}

			var existingOwner = await _userManager.FindByIdAsync(ownerId.ToString());

			if (existingOwner == null)
			{
				_logger.LogWarning("Owner with ID {OwnerId} not found.", request.OwnerId);
				throw new NotFoundException($"Owner with ID {request.OwnerId} not found.");
			}

			// Check role of user is "Host" and "Admin"
			var roles = await _userManager.GetRolesAsync(existingOwner);

			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				_logger.LogWarning("User with ID {OwnerId} does not have the 'Host' or 'Admin' role.", request.OwnerId);
				throw new BadRequestException($"User with ID {request.OwnerId} does not have the 'Host' or 'Admin' role.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Map DTO to entity
				var homestay = _mapper.Map<Homestay>(request);
				homestay.OwnerId = existingOwner.Id;
				homestay.PropertyType = existingPropertyType;
				homestay.IsActive = false; // Mặc định homestay mới tạo là không active, chờ admin duyệt
				homestay.IsApproved = false; // Mặc định homestay mới tạo là không được duyệt, chờ admin duyệt
				homestay.CreatedAt = DateTime.UtcNow;
				homestay.UpdatedAt = DateTime.UtcNow;
				await _homestayRepository.AddAsync(homestay);
				await _homestayRepository.SaveChangesAsync();

				// Create Homestay Images and upload to Cloudinary

				// Check if there are images to upload
				if (request.Images.Count == 0)
				{
					_logger.LogWarning("No images provided for homestay creation.");
					throw new BadRequestException("At least one image is required to create a homestay.");
				}

				// Loop through each image in the request then create HomestayImage entities and upload to Cloudinary
				foreach (var imageDto in request.Images)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(new ImageUploadDto
					{
						File = imageDto.ImageFile,
						Folder = $"{FolderImages.Homestays}/{homestay.Id}"
					});
					if (!uploadResult.Success || uploadResult.Data == null)
					{
						_logger.LogError("Image upload failed: {ErrorMessage}", uploadResult.ErrorMessage);
						throw new BadRequestException($"Image upload failed: {uploadResult.ErrorMessage}");
					}
					// Keep track of successfully uploaded images for potential rollback
					uploadedPublicIds.Add(uploadResult.Data.PublicId);

					// Use AutoMapper to map ImageResponseDto to HomestayImage entity
					var homestayImage = _mapper.Map<HomestayImage>(imageDto);
					homestayImage.HomestayId = homestay.Id;
					homestayImage.ImageUrl = uploadResult.Data.Url;
					await _homestayImageRepository.AddAsync(homestayImage);
				}
				await _homestayImageRepository.SaveChangesAsync();

				// Create Amenities for Homestay
				// Check if there are amenities to add
				if (request.Amenities.Count > 0)
				{
					foreach (var amenity in request.Amenities)
					{
						// Check Id of Amenity is valid
						var existingAmenity = await _amenityRepository.GetByIdAsync(amenity.AmenityId);

						if (existingAmenity == null)
						{
							_logger.LogWarning("Amenity with ID {AmenityId} not found.", amenity.AmenityId);
							throw new NotFoundException($"Amenity with ID {amenity.AmenityId} not found.");
						}

						// Map DTO to entity
						var homestayAmenity = new HomestayAmenity
						{
							HomestayId = homestay.Id,
							AmenityId = amenity.AmenityId,
							CustomNote = amenity.CustomNote,
							IsHighlight = amenity.IsHighlight,
							AssignedAt = DateTime.UtcNow
						};
						await _homestayAmenityRepository.AddAsync(homestayAmenity);
					}
					await _homestayAmenityRepository.SaveChangesAsync();
				}

				// Create Rules for Homestay

				// Check if there are rules to add
				if (request.Rules.Count > 0)
				{
					foreach (var ruleDto in request.Rules)
					{
						// Check Id of Rule is valid
						var existingRule = await _unitOfWork.RuleRepository.GetByIdAsync(ruleDto.RuleId);

						if (existingRule == null)
						{
							_logger.LogWarning("Rule with ID {RuleId} not found.", ruleDto.RuleId);
							throw new NotFoundException($"Rule with ID {ruleDto.RuleId} not found.");
						}

						// Map DTO to entity
						var homestayRule = new HomestayRule
						{
							HomestayId = homestay.Id,
							RuleId = ruleDto.RuleId,
							CustomNote = ruleDto.CustomNote,
							AssignedAt = DateTime.UtcNow
						};

						await _unitOfWork.HomestayRuleRepository.AddAsync(homestayRule);

					}
					await _unitOfWork.HomestayRuleRepository.SaveChangesAsync();
				}

				_logger.LogInformation("Homestay with ID {HomestayId} created successfully.", homestay.Id);
				await _unitOfWork.CommitTransactionAsync();
				return _mapper.Map<HomestayDto>(homestay);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during homestay creation. Initiating rollback.");
				await _unitOfWork.RollbackTransactionAsync();
				// Rollback uploaded images in Cloudinary
				foreach (var publicId in uploadedPublicIds)
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete image with PublicId {PublicId} during rollback.", publicId);
					}
				}
				throw;
			}
		}

	}
}
