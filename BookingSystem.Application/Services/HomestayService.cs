// BookingSystem.Application.Services/HomestayService.cs
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
using BookingSystem.Application.DTOs.HomestayImageDTO;
using BookingSystem.Application.DTOs.HomestayAmenityDTO;
using BookingSystem.Application.DTOs.HomestayRuleDTO;
using BookingSystem.Application.DTOs.AccommodationDTO.BookingSystem.Application.DTOs;
using Microsoft.EntityFrameworkCore;

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

		public HomestayService(
			IUnitOfWork unitOfWork,
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

		#region Basic CRUD Operations

		/// <summary>
		/// Lấy thông tin chi tiết homestay theo ID
		/// </summary>
		public async Task<HomestayDto?> GetByIdAsync(int id)
		{
			_logger.LogInformation("Getting homestay with ID {HomestayId}.", id);

			var homestay = await _homestayRepository.GetHomestayByIdAsync(id);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", id);
				throw new NotFoundException($"Homestay with ID {id} not found.");
			}

			return _mapper.Map<HomestayDto>(homestay);
		}

		/// <summary>
		/// Lấy danh sách homestay có phân trang và filter
		/// </summary>
		public async Task<PagedResult<HomestayDto>> GetAllHomestayAsync(HomestayFilter filter)
		{
			_logger.LogInformation("Getting all homestays with filter.");

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

		/// <summary>
		/// Tạo mới homestay với đầy đủ thông tin
		/// </summary>
		public async Task<HomestayDto?> CreateAsync(int ownerId, CreateHomestayDto request)
		{
			var uploadedPublicIds = new List<string>();

			_logger.LogInformation("Starting homestay creation process for Owner ID {OwnerId}.", ownerId);

			// Validate PropertyType
			var existingPropertyType = await _propertyTypeRepository.GetByIdAsync(request.PropertyTypeId);
			if (existingPropertyType == null)
			{
				_logger.LogWarning("Property type with ID {PropertyTypeId} not found.", request.PropertyTypeId);
				throw new NotFoundException($"Property type with ID {request.PropertyTypeId} not found.");
			}

			// Validate Owner
			var existingOwner = await _userManager.FindByIdAsync(ownerId.ToString());
			if (existingOwner == null)
			{
				_logger.LogWarning("Owner with ID {OwnerId} not found.", ownerId);
				throw new NotFoundException($"Owner with ID {ownerId} not found.");
			}

			// Check role of user is "Host" or "Admin"
			var roles = await _userManager.GetRolesAsync(existingOwner);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				_logger.LogWarning("User with ID {OwnerId} does not have the 'Host' or 'Admin' role.", ownerId);
				throw new BadRequestException($"User with ID {ownerId} does not have the 'Host' or 'Admin' role.");
			}

			// Validate Images
			if (request.Images == null || request.Images.Count == 0)
			{
				_logger.LogWarning("No images provided for homestay creation.");
				throw new BadRequestException("At least one image is required to create a homestay.");
			}

			var homestayId = 0;

			try
			{
				await _unitOfWork.ExecuteInTransactionAsync(async () =>
				{
					// === 1. CREATE HOMESTAY ENTITY ===
					var homestay = _mapper.Map<Homestay>(request);
					homestay.OwnerId = existingOwner.Id;
					homestay.PropertyTypeId = existingPropertyType.Id;
					homestay.IsActive = false; // Chờ admin duyệt
					homestay.IsApproved = false; // Chờ admin duyệt
					homestay.CreatedAt = DateTime.UtcNow;
					homestay.UpdatedAt = DateTime.UtcNow;
					homestay.CreatedBy = existingOwner.Id.ToString();
					homestay.UpdatedBy = existingOwner.Id.ToString();

					await _homestayRepository.AddAsync(homestay);
					await _homestayRepository.SaveChangesAsync();

					_logger.LogInformation("Homestay entity created with ID {HomestayId}.", homestay.Id);

					// === 2. CREATE HOMESTAY IMAGES ===
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

						uploadedPublicIds.Add(uploadResult.Data.PublicId);

						var homestayImage = new HomestayImage
						{
							HomestayId = homestay.Id,
							ImageUrl = uploadResult.Data.Url,
							ImageTitle = imageDto.ImageTitle,
							ImageDescription = imageDto.ImageDescription,
							DisplayOrder = imageDto.DisplayOrder,
							IsPrimaryImage = imageDto.IsPrimaryImage,
							RoomType = imageDto.RoomType,
							CreatedAt = DateTime.UtcNow,
							CreatedBy = existingOwner.Id.ToString()
						};

						await _homestayImageRepository.AddAsync(homestayImage);
					}
					await _homestayImageRepository.SaveChangesAsync();
					_logger.LogInformation("{Count} images uploaded successfully for Homestay ID {HomestayId}.",
						request.Images.Count, homestay.Id);

					// === 3. CREATE AMENITIES ===
					if (request.Amenities != null && request.Amenities.Count > 0)
					{
						foreach (var amenityDto in request.Amenities)
						{
							var existingAmenity = await _amenityRepository.GetByIdAsync(amenityDto.AmenityId);
							if (existingAmenity == null)
							{
								_logger.LogWarning("Amenity with ID {AmenityId} not found.", amenityDto.AmenityId);
								throw new NotFoundException($"Amenity with ID {amenityDto.AmenityId} not found.");
							}

							var homestayAmenity = new HomestayAmenity
							{
								HomestayId = homestay.Id,
								AmenityId = amenityDto.AmenityId,
								CustomNote = amenityDto.CustomNote,
								AssignedAt = DateTime.UtcNow,
								CreatedAt = DateTime.UtcNow,
								CreatedBy = existingOwner.Id.ToString()
							};

							await _homestayAmenityRepository.AddAsync(homestayAmenity);
						}
						await _homestayAmenityRepository.SaveChangesAsync();
						_logger.LogInformation("{Count} amenities added to Homestay ID {HomestayId}.",
							request.Amenities.Count, homestay.Id);
					}

					// === 4. CREATE RULES ===
					if (request.Rules != null && request.Rules.Count > 0)
					{
						foreach (var ruleDto in request.Rules)
						{
							var existingRule = await _ruleRepository.GetByIdAsync(ruleDto.RuleId);
							if (existingRule == null)
							{
								_logger.LogWarning("Rule with ID {RuleId} not found.", ruleDto.RuleId);
								throw new NotFoundException($"Rule with ID {ruleDto.RuleId} not found.");
							}

							var homestayRule = new HomestayRule
							{
								HomestayId = homestay.Id,
								RuleId = ruleDto.RuleId,
								CustomNote = ruleDto.CustomNote,
								AssignedAt = DateTime.UtcNow,
								CreatedAt = DateTime.UtcNow,
								CreatedBy = existingOwner.Id.ToString()
							};

							await _homestayRuleRepository.AddAsync(homestayRule);
						}
						await _homestayRuleRepository.SaveChangesAsync();
						_logger.LogInformation("{Count} rules added to Homestay ID {HomestayId}.",
							request.Rules.Count, homestay.Id);
					}

					// === 5. CREATE AVAILABILITY CALENDARS ===
					if (request.AvailabilityCalendars != null && request.AvailabilityCalendars.Count > 0)
					{
						_logger.LogInformation(
		"Request AvailabilityCalendars count: {Count}",
		request.AvailabilityCalendars.Count
	);

						// ✅ Log TOÀN BỘ dữ liệu với index
						for (int i = 0; i < request.AvailabilityCalendars.Count; i++)
						{
							var cal = request.AvailabilityCalendars[i];
							_logger.LogInformation(
								"[{Index}] Date: {Date}, HomestayId: {HomestayId}, IsAvailable: {IsAvailable}, IsBlocked: {IsBlocked}, HashCode: {Hash}",
								i,
								cal.AvailableDate,
								cal.HomestayId,
								cal.IsAvailable,
								cal.IsBlocked,
								cal.GetHashCode()
							);
						}

						// ✅ Kiểm tra duplicate với DISTINCT
						var distinctDates = request.AvailabilityCalendars
							.Select(c => c.AvailableDate)
							.Distinct()
							.ToList();

						_logger.LogInformation(
							"Distinct dates count: {Count}",
							distinctDates.Count
						);

						if (distinctDates.Count != request.AvailabilityCalendars.Count)
						{
							_logger.LogError(
								"DUPLICATE DETECTED! Original count: {Original}, Distinct count: {Distinct}",
								request.AvailabilityCalendars.Count,
								distinctDates.Count
							);

							// Tìm dates bị duplicate
							var duplicateDates = request.AvailabilityCalendars
								.GroupBy(c => c.AvailableDate)
								.Where(g => g.Count() > 1)
								.Select(g => new { Date = g.Key, Count = g.Count() })
								.ToList();

							foreach (var dup in duplicateDates)
							{
								_logger.LogError(
									"Duplicate date: {Date} appears {Count} times",
									dup.Date,
									dup.Count
								);
							}

							throw new BadRequestException(
								$"Duplicate dates in request: {string.Join(", ", duplicateDates.Select(d => d.Date))}"
							);
						}

						// ✅ BỎ phần GetExistingDatesAsync() - không cần thiết
						// var existingDates = await _unitOfWork.AvailabilityCalendarRepository
						//     .GetExistingDatesAsync(homestay.Id, requestDates);
						// THÊM: Xóa tất cả AvailabilityCalendars hiện có cho homestay này
						var existingCalendars = await _unitOfWork.AvailabilityCalendarRepository
							.FindAsync(x => x.HomestayId == homestay.Id && !x.IsDeleted);
						if (existingCalendars.Any())
						{
							_unitOfWork.AvailabilityCalendarRepository.RemoveRange(existingCalendars);
							_logger.LogInformation(
								"Deleted {Count} existing AvailabilityCalendars for Homestay ID {HomestayId}.",
								existingCalendars.Count(),
								homestay.Id
							);
						}
						// ✅ Insert trực tiếp
						foreach (var calendarDto in request.AvailabilityCalendars)
						{
							var availabilityCalendar = new AvailabilityCalendar
							{
								HomestayId = homestay.Id,
								AvailableDate = calendarDto.AvailableDate, // ← Normalize
								IsAvailable = calendarDto.IsAvailable, // ← Default value
								CustomPrice = calendarDto.CustomPrice,
								MinimumNights = calendarDto.MinimumNights,
								IsBlocked = calendarDto.IsBlocked, // ← Default value
								BlockReason = calendarDto.BlockReason,
								IsDeleted = false, // ← Explicit set
								CreatedAt = DateTime.UtcNow,
								UpdatedAt = DateTime.UtcNow,
								CreatedBy = existingOwner.Id.ToString()
							};
							await _unitOfWork.AvailabilityCalendarRepository.AddAsync(availabilityCalendar);
						}

						//await _unitOfWork.AvailabilityCalendarRepository.SaveChangesAsync();

						_logger.LogInformation(
							"{Count} availability calendars added to Homestay ID {HomestayId}.",
							request.AvailabilityCalendars.Count,
							homestay.Id
						);
					}


					_logger.LogInformation("Homestay with ID {HomestayId} created successfully.", homestay.Id);
					homestayId = homestay.Id;
				});
				// Reload homestay with all relationships
				var createdHomestay = await _homestayRepository.GetByIdAsync(homestayId);
				return _mapper.Map<HomestayDto>(createdHomestay);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during homestay creation. Initiating rollback.");
				await _unitOfWork.RollbackTransactionAsync();

				// Rollback uploaded images in Cloudinary
				foreach (var publicId in uploadedPublicIds)
				{
					try
					{
						var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
						if (!deleteResult.Success)
						{
							_logger.LogWarning("Failed to delete image with PublicId {PublicId} during rollback.", publicId);
						}
					}
					catch (Exception rollbackEx)
					{
						_logger.LogError(rollbackEx, "Error during rollback for PublicId {PublicId}.", publicId);
					}
				}
				throw;
			}
		}

		/// <summary>
		/// Cập nhật thông tin cơ bản của homestay
		/// </summary>
		public async Task<HomestayDto?> UpdateAsync(int homestayId, int ownerId, UpdateHomestayDto request)
		{
			_logger.LogInformation("Starting homestay update process for Homestay ID {HomestayId}.", homestayId);

			// Validate Owner and Permission
			await ValidateOwnerPermissionAsync(ownerId, homestayId, "update");

			var existingOwner = await _userManager.FindByIdAsync(ownerId.ToString());
			var homestayToUpdate = await _homestayRepository.GetByIdAsync(homestayId);

			if (homestayToUpdate == null)
			{
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// Validate PropertyType if provided
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

				// Update basic fields (only if provided)
				if (!string.IsNullOrWhiteSpace(request.HomestayTitle))
					homestayToUpdate.HomestayTitle = request.HomestayTitle;

				if (request.HomestayDescription != null)
					homestayToUpdate.HomestayDescription = request.HomestayDescription;

				if (!string.IsNullOrWhiteSpace(request.FullAddress))
					homestayToUpdate.FullAddress = request.FullAddress;

				if (!string.IsNullOrWhiteSpace(request.City))
					homestayToUpdate.City = request.City;

				if (!string.IsNullOrWhiteSpace(request.Province))
					homestayToUpdate.Province = request.Province;

				if (!string.IsNullOrWhiteSpace(request.Country))
					homestayToUpdate.Country = request.Country;

				if (request.PostalCode != null)
					homestayToUpdate.PostalCode = request.PostalCode;

				if (request.Latitude.HasValue)
					homestayToUpdate.Latitude = request.Latitude.Value;

				if (request.Longitude.HasValue)
					homestayToUpdate.Longitude = request.Longitude.Value;

				if (request.MaximumGuests.HasValue)
					homestayToUpdate.MaximumGuests = request.MaximumGuests.Value;

				if (request.NumberOfBedrooms.HasValue)
					homestayToUpdate.NumberOfBedrooms = request.NumberOfBedrooms.Value;

				if (request.NumberOfBathrooms.HasValue)
					homestayToUpdate.NumberOfBathrooms = request.NumberOfBathrooms.Value;

				if (request.NumberOfBeds.HasValue)
					homestayToUpdate.NumberOfBeds = request.NumberOfBeds.Value;

				if (request.BaseNightlyPrice.HasValue)
					homestayToUpdate.BaseNightlyPrice = request.BaseNightlyPrice.Value;

				if (request.WeekendPrice.HasValue)
					homestayToUpdate.WeekendPrice = request.WeekendPrice.Value;

				if (request.WeeklyDiscount.HasValue)
					homestayToUpdate.WeeklyDiscount = request.WeeklyDiscount.Value;

				if (request.MonthlyDiscount.HasValue)
					homestayToUpdate.MonthlyDiscount = request.MonthlyDiscount.Value;

				if (request.MinimumNights.HasValue)
					homestayToUpdate.MinimumNights = request.MinimumNights.Value;

				if (request.MaximumNights.HasValue)
					homestayToUpdate.MaximumNights = request.MaximumNights.Value;

				if (request.CheckInTime.HasValue)
					homestayToUpdate.CheckInTime = request.CheckInTime.Value;

				if (request.CheckOutTime.HasValue)
					homestayToUpdate.CheckOutTime = request.CheckOutTime.Value;

				if (request.IsInstantBook.HasValue)
					homestayToUpdate.IsInstantBook = request.IsInstantBook.Value;

				homestayToUpdate.UpdatedAt = DateTime.UtcNow;
				homestayToUpdate.UpdatedBy = existingOwner!.Id.ToString();

				_homestayRepository.Update(homestayToUpdate);
				await _homestayRepository.SaveChangesAsync();

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Homestay with ID {HomestayId} updated successfully.", homestayId);

				// Reload homestay with all relationships
				var updatedHomestay = await _homestayRepository.GetByIdAsync(homestayId);
				return _mapper.Map<HomestayDto>(updatedHomestay);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during homestay update. Initiating rollback.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		/// <summary>
		/// Xóa homestay (Admin only)
		/// </summary>
		public async Task<bool> DeleteAsync(int id)
		{
			_logger.LogInformation("Starting homestay deletion process for Homestay ID {Id}.", id);

			var homestay = await _homestayRepository.GetByIdAsync(id);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", id);
				throw new NotFoundException($"Homestay with ID {id} not found.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// === 1. DELETE IMAGES FROM CLOUDINARY AND DATABASE ===
				var homestayImages = await _homestayImageRepository.GetByHomestayIdAsync(homestay.Id);
				foreach (var image in homestayImages)
				{
					var publicId = _cloudinaryService.GetPublicIdFromUrl(image.ImageUrl);
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete image with PublicId {PublicId} from Cloudinary.", publicId);
						// Continue deletion process even if Cloudinary delete fails
					}
					_homestayImageRepository.Remove(image);
				}
				await _homestayImageRepository.SaveChangesAsync();
				_logger.LogInformation("Deleted {Count} images for Homestay ID {HomestayId}.",
					homestayImages.Count(), id);

				// === 2. DELETE AMENITIES ===
				var homestayAmenities = await _homestayAmenityRepository.GetByHomestayIdAsync(homestay.Id);
				foreach (var amenity in homestayAmenities)
				{
					_homestayAmenityRepository.Remove(amenity);
				}
				await _homestayAmenityRepository.SaveChangesAsync();
				_logger.LogInformation("Deleted {Count} amenities for Homestay ID {HomestayId}.",
					homestayAmenities.Count(), id);

				// === 3. DELETE RULES ===
				var homestayRules = await _homestayRuleRepository.GetByHomestayIdAsync(homestay.Id);
				foreach (var rule in homestayRules)
				{
					_homestayRuleRepository.Remove(rule);
				}
				await _homestayRuleRepository.SaveChangesAsync();
				_logger.LogInformation("Deleted {Count} rules for Homestay ID {HomestayId}.",
					homestayRules.Count(), id);

				// === 4. DELETE AVAILABILITY CALENDARS ===
				var availabilityCalendars = await _unitOfWork.AvailabilityCalendarRepository
					.FindAsync(ac => ac.HomestayId == homestay.Id);
				foreach (var calendar in availabilityCalendars)
				{
					_unitOfWork.AvailabilityCalendarRepository.Remove(calendar);
				}
				await _unitOfWork.AvailabilityCalendarRepository.SaveChangesAsync();
				_logger.LogInformation("Deleted {Count} availability calendars for Homestay ID {HomestayId}.",
					availabilityCalendars.Count(), id);

				// === 5. DELETE HOMESTAY ===
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

		#endregion

		#region Status Management

		/// <summary>
		/// Kích hoạt homestay (Owner hoặc Admin)
		/// </summary>
		public async Task<bool> ActivateAsync(int homestayId, int userActiveId)
		{
			_logger.LogInformation("Activating Homestay ID {HomestayId} by User ID {UserId}.",
				homestayId, userActiveId);

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

			// Validate user permission
			await ValidateOwnerPermissionAsync(userActiveId, homestayId, "activate");

			var user = await _userManager.FindByIdAsync(userActiveId.ToString());

			homestay.IsActive = true;
			homestay.UpdatedAt = DateTime.UtcNow;
			homestay.UpdatedBy = user!.Id.ToString();

			_homestayRepository.Update(homestay);
			await _homestayRepository.SaveChangesAsync();

			_logger.LogInformation("Homestay with ID {HomestayId} activated successfully.", homestayId);
			return true;
		}

		/// <summary>
		/// Vô hiệu hóa homestay (Owner hoặc Admin)
		/// </summary>
		public async Task<bool> DeactivateAsync(int homestayId, int userActiveId)
		{
			_logger.LogInformation("Deactivating Homestay ID {HomestayId} by User ID {UserId}.",
				homestayId, userActiveId);

			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			// Validate user permission
			await ValidateOwnerPermissionAsync(userActiveId, homestayId, "deactivate");

			var user = await _userManager.FindByIdAsync(userActiveId.ToString());

			homestay.IsActive = false;
			homestay.UpdatedAt = DateTime.UtcNow;
			homestay.UpdatedBy = user!.Id.ToString();

			_homestayRepository.Update(homestay);
			await _homestayRepository.SaveChangesAsync();

			_logger.LogInformation("Homestay with ID {HomestayId} deactivated successfully.", homestayId);
			return true;
		}

		#endregion

		#region Image Management

		/// <summary>
		/// Cập nhật hình ảnh của homestay
		/// </summary>
		public async Task<bool> UpdateHomestayImages(int homestayId, int ownerId, UpdateHomestayImagesDto updateHomestayImages)
		{
			_logger.LogInformation("Updating images for Homestay ID {HomestayId}.", homestayId);

			// Validate Owner and Permission
			await ValidateOwnerPermissionAsync(ownerId, homestayId, "update images");

			var existingOwner = await _userManager.FindByIdAsync(ownerId.ToString());
			var homestayToUpdate = await _homestayRepository.GetByIdAsync(homestayId);

			if (homestayToUpdate == null)
			{
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			var uploadedPublicIds = new List<string>();

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				var existingImages = await _homestayImageRepository.GetByHomestayIdAsync(homestayId);

				// === 1. DELETE IMAGES NOT IN KeepImageIds ===
				var imagesToDelete = existingImages
					.Where(img => !updateHomestayImages.KeepImageIds.Contains(img.Id))
					.ToList();

				var publicIdsToDelete = imagesToDelete
					.Select(img => _cloudinaryService.GetPublicIdFromUrl(img.ImageUrl))
					.ToList();

				foreach (var publicId in publicIdsToDelete)
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (!deleteResult.Success)
					{
						_logger.LogWarning("Failed to delete image with PublicId {PublicId} from Cloudinary.", publicId);
						// Continue anyway
					}
				}

				foreach (var image in imagesToDelete)
				{
					_homestayImageRepository.Remove(image);
				}
				await _homestayImageRepository.SaveChangesAsync();
				_logger.LogInformation("Deleted {Count} images for Homestay ID {HomestayId}.",
					imagesToDelete.Count, homestayId);

				// === 2. UPLOAD NEW IMAGES ===
				if (updateHomestayImages.NewImages != null && updateHomestayImages.NewImages.Count > 0)
				{
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

						uploadedPublicIds.Add(uploadResult.Data.PublicId);

						var homestayImage = new HomestayImage
						{
							HomestayId = homestayToUpdate.Id,
							ImageUrl = uploadResult.Data.Url,
							ImageTitle = imageDto.ImageTitle,
							ImageDescription = imageDto.ImageDescription,
							DisplayOrder = imageDto.DisplayOrder,
							IsPrimaryImage = imageDto.IsPrimaryImage,
							RoomType = imageDto.RoomType,
							CreatedAt = DateTime.UtcNow,
							CreatedBy = existingOwner!.Id.ToString()
						};

						await _homestayImageRepository.AddAsync(homestayImage);
					}
					await _homestayImageRepository.SaveChangesAsync();
					_logger.LogInformation("Uploaded {Count} new images for Homestay ID {HomestayId}.",
						updateHomestayImages.NewImages.Count, homestayId);
				}

				// === 3. UPDATE EXISTING IMAGES METADATA ===
				if (updateHomestayImages.UpdateExistingImages != null &&
					updateHomestayImages.UpdateExistingImages.Count > 0)
				{
					foreach (var updateImage in updateHomestayImages.UpdateExistingImages)
					{
						var existingImage = existingImages.FirstOrDefault(img => img.Id == updateImage.ImageId);
						if (existingImage == null) continue;

						if (!string.IsNullOrWhiteSpace(updateImage.ImageTitle))
							existingImage.ImageTitle = updateImage.ImageTitle;

						if (updateImage.ImageDescription != null)
							existingImage.ImageDescription = updateImage.ImageDescription;

						if (updateImage.DisplayOrder.HasValue)
							existingImage.DisplayOrder = updateImage.DisplayOrder.Value;

						if (updateImage.IsPrimaryImage.HasValue)
							existingImage.IsPrimaryImage = updateImage.IsPrimaryImage.Value;

						if (!string.IsNullOrWhiteSpace(updateImage.RoomType))
							existingImage.RoomType = updateImage.RoomType;

						existingImage.UpdatedAt = DateTime.UtcNow;
						existingImage.UpdatedBy = existingOwner!.Id.ToString();

						_homestayImageRepository.Update(existingImage);
					}
					await _homestayImageRepository.SaveChangesAsync();
					_logger.LogInformation("Updated metadata for {Count} existing images of Homestay ID {HomestayId}.",
						updateHomestayImages.UpdateExistingImages.Count, homestayId);
				}

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Homestay images for Homestay ID {HomestayId} updated successfully.", homestayId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during homestay images update. Initiating rollback.");
				await _unitOfWork.RollbackTransactionAsync();

				// Rollback uploaded images in Cloudinary
				foreach (var publicId in uploadedPublicIds)
				{
					try
					{
						var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
						if (!deleteResult.Success)
						{
							_logger.LogWarning("Failed to delete image with PublicId {PublicId} during rollback.", publicId);
						}
					}
					catch (Exception rollbackEx)
					{
						_logger.LogError(rollbackEx, "Error during rollback for PublicId {PublicId}.", publicId);
					}
				}
				throw;
			}
		}

		#endregion

		#region Amenities Management

		/// <summary>
		/// Cập nhật danh sách Amenities cho Homestay
		/// </summary>
		public async Task<bool> UpdateHomestayAmenitiesAsync(int homestayId, int ownerId, List<CreateHomestayAmenityDto> amenities)
		{
			_logger.LogInformation("Updating amenities for Homestay ID {HomestayId}.", homestayId);

			// Validate Owner and Permission
			await ValidateOwnerPermissionAsync(ownerId, homestayId, "update amenities");

			var existingOwner = await _userManager.FindByIdAsync(ownerId.ToString());
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);

			if (homestay == null)
			{
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// === 1. REMOVE ALL EXISTING AMENITIES ===
				var existingAmenities = await _homestayAmenityRepository.GetByHomestayIdAsync(homestayId);
				foreach (var amenity in existingAmenities)
				{
					_homestayAmenityRepository.Remove(amenity);
				}
				await _homestayAmenityRepository.SaveChangesAsync();
				_logger.LogInformation("Removed {Count} existing amenities from Homestay ID {HomestayId}.",
					existingAmenities.Count(), homestayId);

				// === 2. ADD NEW AMENITIES ===
				if (amenities != null && amenities.Count > 0)
				{
					foreach (var amenityDto in amenities)
					{
						var existingAmenity = await _amenityRepository.GetByIdAsync(amenityDto.AmenityId);
						if (existingAmenity == null)
						{
							_logger.LogWarning("Amenity with ID {AmenityId} not found.", amenityDto.AmenityId);
							throw new NotFoundException($"Amenity with ID {amenityDto.AmenityId} not found.");
						}

						if (!existingAmenity.IsActive)
						{
							_logger.LogWarning("Amenity with ID {AmenityId} is not active.", amenityDto.AmenityId);
							throw new BadRequestException($"Amenity with ID {amenityDto.AmenityId} is not active.");
						}

						var homestayAmenity = new HomestayAmenity
						{
							HomestayId = homestayId,
							AmenityId = amenityDto.AmenityId,
							CustomNote = amenityDto.CustomNote,
							AssignedAt = DateTime.UtcNow,
							CreatedAt = DateTime.UtcNow,
							CreatedBy = existingOwner!.Id.ToString()
						};

						await _homestayAmenityRepository.AddAsync(homestayAmenity);
					}
					await _homestayAmenityRepository.SaveChangesAsync();
					_logger.LogInformation("Added {Count} new amenities to Homestay ID {HomestayId}.",
						amenities.Count, homestayId);
				}

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Amenities updated successfully for Homestay ID {HomestayId}.", homestayId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during amenities update.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		#endregion

		#region Rules Management

		/// <summary>
		/// Cập nhật danh sách Rules cho Homestay
		/// </summary>
		public async Task<bool> UpdateHomestayRulesAsync(int homestayId, int ownerId, List<CreateHomestayRuleDto> rules)
		{
			_logger.LogInformation("Updating rules for Homestay ID {HomestayId}.", homestayId);

			// Validate Owner and Permission
			await ValidateOwnerPermissionAsync(ownerId, homestayId, "update rules");

			var existingOwner = await _userManager.FindByIdAsync(ownerId.ToString());
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);

			if (homestay == null)
			{
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();

				// === 1. REMOVE ALL EXISTING RULES ===
				var existingRules = await _homestayRuleRepository.GetByHomestayIdAsync(homestayId);
				foreach (var rule in existingRules)
				{
					_homestayRuleRepository.Remove(rule);
				}
				await _homestayRuleRepository.SaveChangesAsync();
				_logger.LogInformation("Removed {Count} existing rules from Homestay ID {HomestayId}.",
					existingRules.Count(), homestayId);

				// === 2. ADD NEW RULES ===
				if (rules != null && rules.Count > 0)
				{
					foreach (var ruleDto in rules)
					{
						var existingRule = await _ruleRepository.GetByIdAsync(ruleDto.RuleId);
						if (existingRule == null)
						{
							_logger.LogWarning("Rule with ID {RuleId} not found.", ruleDto.RuleId);
							throw new NotFoundException($"Rule with ID {ruleDto.RuleId} not found.");
						}

						if (!existingRule.IsActive)
						{
							_logger.LogWarning("Rule with ID {RuleId} is not active.", ruleDto.RuleId);
							throw new BadRequestException($"Rule with ID {ruleDto.RuleId} is not active.");
						}

						var homestayRule = new HomestayRule
						{
							HomestayId = homestayId,
							RuleId = ruleDto.RuleId,
							CustomNote = ruleDto.CustomNote,
							AssignedAt = DateTime.UtcNow,
							CreatedAt = DateTime.UtcNow,
							CreatedBy = existingOwner!.Id.ToString()
						};

						await _homestayRuleRepository.AddAsync(homestayRule);
					}
					await _homestayRuleRepository.SaveChangesAsync();
					_logger.LogInformation("Added {Count} new rules to Homestay ID {HomestayId}.",
						rules.Count, homestayId);
				}

				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation("Rules updated successfully for Homestay ID {HomestayId}.", homestayId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during rules update.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		#endregion

		#region Private Helper Methods

		/// <summary>
		/// Validate user permission for homestay operations
		/// </summary>
		private async Task ValidateOwnerPermissionAsync(int userId, int homestayId, string action)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				_logger.LogWarning("User with ID {UserId} not found.", userId);
				throw new NotFoundException($"User with ID {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			if (!roles.Contains("Host") && !roles.Contains("Admin"))
			{
				_logger.LogWarning("User with ID {UserId} does not have the 'Host' or 'Admin' role.", userId);
				throw new BadRequestException($"User with ID {userId} does not have the 'Host' or 'Admin' role.");
			}

			// Admin can perform any action
			if (roles.Contains("Admin"))
			{
				return;
			}

			// Host can only perform action on their own homestay
			var homestay = await _homestayRepository.GetByIdAsync(homestayId);
			if (homestay == null)
			{
				_logger.LogWarning("Homestay with ID {HomestayId} not found.", homestayId);
				throw new NotFoundException($"Homestay with ID {homestayId} not found.");
			}

			if (homestay.OwnerId != user.Id)
			{
				_logger.LogWarning("User with ID {UserId} does not have permission to {Action} this homestay.",
					userId, action);
				throw new BadRequestException($"User with ID {userId} does not have permission to {action} this homestay.");
			}
		}

		#endregion
	}
}