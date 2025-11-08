using BookingSystem.Application.Contracts;
using BookingSystem.Domain.Base;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BookingSystem.Domain.Repositories;
using BookingSystem.Application.DTOs.HostProfileDTO;
using Microsoft.AspNetCore.Identity;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Exceptions;
using BookingSystem.Domain.Enums;
using BookingSystem.Application.DTOs.ImageDTO;
using BookingSystem.Application.Models.Constants;
using BookingSystem.Domain.Base.Filter;

namespace BookingSystem.Application.Services
{
    public class HostProfileService : IHostProfileService
    {
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHostProfileRepository _hostProfileRepository;
		private readonly IMapper _mapper;
		private readonly ICloudinaryService _cloudinaryService;
		private readonly ILogger<HostProfileService> _logger;
		private readonly UserManager<User> _userManager;
		public HostProfileService(IUnitOfWork unitOfWork, 
									IHostProfileRepository hostProfileRepository, 
									IMapper mapper, 
									ICloudinaryService cloudinaryService, 
									UserManager<User> userManager,
									ILogger<HostProfileService> logger)
		{
			_unitOfWork = unitOfWork;
			_hostProfileRepository = hostProfileRepository;
			_mapper = mapper;
			_cloudinaryService = cloudinaryService;
			_logger = logger;
			_userManager = userManager;
		}

		public async Task<bool> UpdateHostProfileAsync(int hostProfileId, int currentUserId, UpdateHostProfileDto dto)
		{
			var host = await _hostProfileRepository.GetByIdAsync(hostProfileId);

			// Check if host exists
			if (host == null)
			{
				_logger.LogWarning($"Host with ID {hostProfileId} not found.");
				throw new NotFoundException($"Host with ID: {hostProfileId} not found.");
			}

			var user = await _userManager.FindByIdAsync(currentUserId.ToString());
			var roles = await _userManager.GetRolesAsync(user);
			var isHost = roles.Any(r => r.Equals("Host", StringComparison.OrdinalIgnoreCase));
			var isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));


			var isCurrentUser = currentUserId == user.Id;
			if (!isCurrentUser && !isHost && !isAdmin)
			{
				_logger.LogWarning("User with ID {CurrentUserId} is not authorized to update host profile {HostUserId}.", currentUserId, hostProfileId);
				throw new UnauthorizedAccessException("You do not have permission to update this host profile.");
			}

			// Map updated fields from DTO to entity
			_mapper.Map(dto, host);
			host.UpdatedAt = DateTime.UtcNow;
			_hostProfileRepository.Update(host);
			await _unitOfWork.SaveChangesAsync();
			_logger.LogInformation($"Host with ID:  {hostProfileId} updated successfully.");
			return true;
		}

		public async Task<PagedResult<HostProfileDto>> GetAllHostProfileAsync(HostProfileFilter hostProfileFilter)
		{
			var pagedHostProfiles = await _hostProfileRepository.GetPagedHostProfilesAsync(hostProfileFilter);
			var hostProfileDtos = _mapper.Map<List<HostProfileDto>>(pagedHostProfiles.Items);
			_logger.LogInformation($"Retrieved {hostProfileDtos.Count} host profiles with filter: {hostProfileFilter}");
			return new PagedResult<HostProfileDto>
			{
				Items = hostProfileDtos,
				TotalCount = pagedHostProfiles.TotalCount,
				PageSize = pagedHostProfiles.PageSize,
				PageNumber = pagedHostProfiles.PageNumber,
				TotalPages = pagedHostProfiles.TotalPages
			};
		}

		public async Task<string> UploadTaxCodeDocumentAsync(int hostId, UploadTaxCodeDocumentDto documentDto)
		{
			var hostProfile = await _hostProfileRepository.GetByIdAsync(hostId);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {hostId} not found.");
				throw new NotFoundException($"Host profile with ID: {hostId} not found.");
			}
			try
			{
				var uploadResult = await _cloudinaryService.UploadImageAsync(
					new ImageUploadDto
					{
						File = documentDto.File,
						Folder = $"{FolderImages.Lisenses}/{hostProfile.Id}"
					}
				);
				if (!uploadResult.Success)
				{
					_logger.LogError($"Failed to upload tax code document image: {uploadResult.ErrorMessage}");
					throw new Exception($"Failed to upload tax code document image: {uploadResult.ErrorMessage}");
				}
				hostProfile.TaxCodeDocumentUrl = uploadResult.Data.Url;
				hostProfile.UpdatedAt = DateTime.UtcNow;
				_hostProfileRepository.Update(hostProfile);
				await _unitOfWork.SaveChangesAsync();
				_logger.LogInformation($"Tax code document image uploaded successfully for host ID {hostProfile.Id}");
				return "Tax code document image uploaded successfully.";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while uploading tax code document image.");
				throw;
			}
		}

		public async Task<string> UploadBusinessLicenseAsync(int hostId, UploadBusinessLicenseDto licenseDto)
		{
			var hostProfile = await _hostProfileRepository.GetByIdAsync(hostId);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {hostId} not found.");
				throw new NotFoundException($"Host profile with ID: {hostId} not found.");
			}
			try
			{
				var uploadResult = await _cloudinaryService.UploadImageAsync(
					new ImageUploadDto
					{
						File = licenseDto.File,
						Folder = $"{FolderImages.Lisenses}/{hostProfile.Id}"
					}
				);
				if (!uploadResult.Success)
				{
					_logger.LogError($"Failed to upload business license image: {uploadResult.ErrorMessage}");
					throw new Exception($"Failed to upload business license image: {uploadResult.ErrorMessage}");
				}
				hostProfile.BusinessLicenseUrl = uploadResult.Data.Url;
				hostProfile.UpdatedAt = DateTime.UtcNow;
				_hostProfileRepository.Update(hostProfile);
				await _unitOfWork.SaveChangesAsync();
				_logger.LogInformation($"Business license image uploaded successfully for host ID {hostProfile.Id}");
				return "Business license image uploaded successfully.";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while uploading business license image.");
				throw;
			}
		}

		public async Task<string> UploadIdentityCardAsync(int hostId, UploadIdentityCardDto dto)
		{
			var hostProfile = await _hostProfileRepository.GetByIdAsync(hostId);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {hostId} not found.");
				throw new NotFoundException($"Host profile with ID: {hostId} not found.");
			}
			var uploadedImages = new List<string>();
			try
			{
				// Upload front side of ID card
				var frontUploadResult = await _cloudinaryService.UploadImageAsync(
					new ImageUploadDto
					{
						File = dto.FrontImage,
						Folder = $"{FolderImages.Hosts}/{hostProfile.Id}"
					}
				);
				if (!frontUploadResult.Success)
				{
					_logger.LogError($"Failed to upload front ID card image: {frontUploadResult.ErrorMessage}");
					throw new Exception($"Failed to upload front ID card image: {frontUploadResult.ErrorMessage}");
				}
				hostProfile.IdentityCardFrontUrl = frontUploadResult.Data.Url;
				uploadedImages.Add(frontUploadResult.Data.PublicId);
				_logger.LogInformation($"Front ID card image uploaded successfully for host ID {hostProfile.Id}");
				// Upload back side of ID card
				var backUploadResult = await _cloudinaryService.UploadImageAsync(
					new ImageUploadDto
					{
						File = dto.BackImage,
						Folder = $"{FolderImages.Hosts}/{hostProfile.Id}"
					}
				);
				if (!backUploadResult.Success)
				{
					_logger.LogError($"Failed to upload back ID card image: {backUploadResult.ErrorMessage}");
					throw new Exception($"Failed to upload back ID card image: {backUploadResult.ErrorMessage}");
				}
				hostProfile.IdentityCardBackUrl = backUploadResult.Data.Url;
				uploadedImages.Add(backUploadResult.Data.PublicId);
				_logger.LogInformation($"Back ID card image uploaded successfully for host ID {hostProfile.Id}");
				hostProfile.UpdatedAt = DateTime.UtcNow;
				_hostProfileRepository.Update(hostProfile);
				await _unitOfWork.SaveChangesAsync();
				return "Identity card images uploaded successfully.";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while uploading identity card images.");
				// Cleanup uploaded images in case of failure
				foreach (var publicId in uploadedImages)
				{
					await _cloudinaryService.DeleteImageAsync(publicId);
					_logger.LogInformation($"Rolled back uploaded image with public ID {publicId}");
				}
				throw;
			}
		}

		public async Task UpdateStatisticsAsync(int hostProfileId, int totalHomestays, int totalBookings, decimal averageRating, int responseRate, TimeSpan? avgResponseTime)
		{
			var hostProfile = await _hostProfileRepository.GetByIdAsync(hostProfileId);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {hostProfileId} not found.");
				throw new NotFoundException($"Host profile with ID: {hostProfileId} not found.");
			}
			hostProfile.TotalHomestays = totalHomestays;
			hostProfile.TotalBookings = totalBookings;
			hostProfile.AverageRating = averageRating;
			hostProfile.ResponseRate = responseRate;
			hostProfile.AverageResponseTime = avgResponseTime;
			hostProfile.UpdatedAt = DateTime.UtcNow;
			_hostProfileRepository.Update(hostProfile);
			await _unitOfWork.SaveChangesAsync();
			_logger.LogInformation($"Host profile statistics updated for ID {hostProfileId}.");
		}

		public async Task<bool> ReviewHostProfileAsync(int id, int adminId, string status, string? note)
		{
			// Retrieve host profile
			var hostProfile = await _hostProfileRepository.GetByIdAsync(id);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {id} not found.");
				throw new NotFoundException($"Host profile with ID: {id} not found.");
			}
			// Check Admin existence
			var adminUser = await _userManager.FindByIdAsync(adminId.ToString());
			if (adminUser == null || !await _userManager.IsInRoleAsync(adminUser, "Admin"))
			{
				_logger.LogWarning($"Admin user with ID {adminId} not found or is not an admin.");
				throw new UnauthorizedAccessException("Only admins can review host profiles.");
			}
			// Update status and audit fields
			if (Enum.TryParse<HostStatus>(status, true, out var parsedStatus))
			{
				hostProfile.Status = parsedStatus;
			}
			else
			{
				_logger.LogWarning($"Invalid status value: {status}");
				throw new ArgumentException($"Invalid status value: {status}");
			}
			hostProfile.UpdatedAt = DateTime.UtcNow;
			hostProfile.ReviewedByAdminId = adminId;
			hostProfile.ReviewNote = note;
			hostProfile.ReviewedAt = DateTime.UtcNow;
			_hostProfileRepository.Update(hostProfile);
			await _unitOfWork.SaveChangesAsync();
			_logger.LogInformation($"Host profile with ID {id} reviewed by admin ID {adminId} with status {status}.");
			return true;
		}

		public async Task<bool> MarkAsSuperhostAsync(int hostProfileId, int adminId, bool isSuperhost)
		{
			// Retrieve host profile
			var hostProfile = await _hostProfileRepository.GetByIdAsync(hostProfileId);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {hostProfileId} not found.");
				throw new NotFoundException($"Host profile with ID: {hostProfileId} not found.");
			}
			// Check Admin existence
			var adminUser = await _userManager.FindByIdAsync(adminId.ToString());
			if (adminUser == null || !await _userManager.IsInRoleAsync(adminUser, "Admin"))
			{
				_logger.LogWarning($"Admin user with ID {adminId} not found or is not an admin.");
				throw new UnauthorizedAccessException("Only admins can change superhost status.");
			}
			// Update superhost status
			hostProfile.IsSuperhost = isSuperhost;
			hostProfile.ReviewedByAdminId = adminId;
			hostProfile.ReviewedAt = DateTime.UtcNow;
			hostProfile.UpdatedAt = DateTime.UtcNow;
			_hostProfileRepository.Update(hostProfile);

			await _unitOfWork.SaveChangesAsync();
			_logger.LogInformation($"Host profile with ID {hostProfileId} superhost status set to {isSuperhost}.");
			return true;
		}

		public async Task<bool> ToggleActiveStatusAsync(int hostProfileid, int adminId, bool isActive)
		{
			// Retrieve host profile
			var hostProfile = await _hostProfileRepository.GetByIdAsync(hostProfileid);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {hostProfileid} not found.");
				throw new NotFoundException($"Host profile with ID: {hostProfileid} not found.");
			}
			// Check Admin existence
			var adminUser = await _userManager.FindByIdAsync(adminId.ToString());
			if (adminUser == null || !await _userManager.IsInRoleAsync(adminUser, "Admin"))
			{
				_logger.LogWarning($"Admin user with ID {adminId} not found or is not an admin.");
				throw new UnauthorizedAccessException("Only admins can change host active status.");
			}
			// Update active status
			hostProfile.IsActive = isActive;
			hostProfile.UpdatedAt = DateTime.UtcNow;
			hostProfile.ReviewedByAdminId = adminId;
			hostProfile.ReviewedAt = DateTime.UtcNow;

			_hostProfileRepository.Update(hostProfile);
			await _unitOfWork.SaveChangesAsync();
			_logger.LogInformation($"Host profile with ID {hostProfileid} active status set to {isActive}.");
			return true;
		}

		public async Task<bool> RejectHostProfileAsync(int id, int adminId, string reason)
		{
			// Retrieve host profile
			var hostProfile = await _hostProfileRepository.GetByIdAsync(id);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {id} not found.");
				throw new NotFoundException($"Host profile with ID: {id} not found.");
			}
			// Check Admin existence
			var adminUser = await _userManager.FindByIdAsync(adminId.ToString());
			if (adminUser == null || !await _userManager.IsInRoleAsync(adminUser, "Admin"))
			{
				_logger.LogWarning($"Admin user with ID {adminId} not found or is not an admin.");
				throw new UnauthorizedAccessException("Only admins can reject host profiles.");
			}
			// Update status and audit fields
			hostProfile.Status = HostStatus.Rejected;
			hostProfile.IsActive = false;
			hostProfile.UpdatedAt = DateTime.UtcNow;
			hostProfile.ReviewedByAdminId = adminId;
			hostProfile.ReviewNote = reason;
			hostProfile.ReviewedAt = DateTime.UtcNow;
			_hostProfileRepository.Update(hostProfile);
			await _unitOfWork.SaveChangesAsync();
			_logger.LogInformation($"Host profile with ID {id} rejected by admin ID {adminId} for reason: {reason}");
			return true;
		}

		public async Task<bool> ApproveHostProfileAsync(int hostProfileid, int adminId, string? note = null)
		{
			// Retrieve host profile
			var hostProfile = await _hostProfileRepository.GetByIdAsync(hostProfileid);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile with ID {hostProfileid} not found.");
				throw new NotFoundException($"Host profile with ID: {hostProfileid} not found.");
			}

			// Check Admin existence
			var adminUser = await _userManager.FindByIdAsync(adminId.ToString());
			if (adminUser == null || !await _userManager.IsInRoleAsync(adminUser, "Admin"))
			{
				_logger.LogWarning($"Admin user with ID {adminId} not found or is not an admin.");
				throw new UnauthorizedAccessException("Only admins can approve host profiles.");
			}

			// Update status and audit fields
			hostProfile.Status = HostStatus.Approved;
			hostProfile.IsActive = true;
			hostProfile.UpdatedAt = DateTime.UtcNow;
			hostProfile.ReviewedByAdminId = adminId;
			hostProfile.ReviewNote = note;
			hostProfile.ReviewedAt = DateTime.UtcNow;

			// Thêm Role Host cho user
			var user = await _userManager.FindByIdAsync(hostProfile.UserId.ToString());
			if (user == null)
			{
				_logger.LogWarning($"User with ID {hostProfile.UserId} not found.");
				throw new NotFoundException($"User with ID: {hostProfile.UserId} not found.");
			}

			if (!await _userManager.IsInRoleAsync(user, "Host"))
			{
				var roleResult = await _userManager.AddToRoleAsync(user, "Host");
				if (!roleResult.Succeeded)
				{
					_logger.LogError($"Failed to assign 'Host' role to user ID {user.Id}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
					throw new Exception($"Failed to assign 'Host' role to user ID {user.Id}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
				}
				_logger.LogInformation($"'Host' role assigned to user ID {user.Id} successfully.");
			}

			_hostProfileRepository.Update(hostProfile);
			await _unitOfWork.SaveChangesAsync();
			_logger.LogInformation($"Host profile with ID {hostProfileid} approved by admin ID {adminId}.");
			return true;
		}

		public async Task<HostProfileDto?> GetHostProfileByIdAsync(int userId, int currentUserId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());

			// Check if user exists
			if (user == null)
			{
				_logger.LogWarning($"User with ID {userId} not found.");
				throw new NotFoundException($"User with ID: {userId} not found.");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var isHost = roles.Any(r => r.Equals("Host", StringComparison.OrdinalIgnoreCase));
			var isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));


			var isCurrentUser = currentUserId == userId;
			if (!isCurrentUser && !isHost && !isAdmin)
			{
				_logger.LogWarning("User {CurrentUserId} attempted to access profile of user {UserId} without permission.", currentUserId, userId);
				throw new UnauthorizedAccessException("You are not authorized to view this host profile.");
			}

			// Retrieve host profile
			var hostProfile = await _hostProfileRepository.GetByUserIdAsync(userId);

			// Check if host profile exists
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host profile for user ID {userId} not found.");
				throw new NotFoundException($"Host profile for user ID: {userId} not found.");
			}

			_logger.LogInformation($"Host profile for user ID {userId} retrieved successfully.");
			// Map to DTO and return
			return _mapper.Map<HostProfileDto>(hostProfile);
		}

		public async Task<bool> RegisterHostAsync(CreateHostProfileDto dto)
		{
			var uploadedImages = new List<string>();

			var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
			// Check if user exists
			if (user == null)
			{
				_logger.LogWarning($"User with ID {dto.UserId} not found.");
				throw new NotFoundException($"User with ID: {dto.UserId} not found.");
			}
			// Check if user is already a host
			if (await _hostProfileRepository.GetByUserIdAsync(dto.UserId) != null)
			{
				_logger.LogWarning($"User with ID {dto.UserId} is already registered as a host.");
				throw new InvalidOperationException($"User with ID: {dto.UserId} is already registered as a host.");
			}
			// Map DTO to entity
			var hostProfile = _mapper.Map<HostProfile>(dto);
			hostProfile.Status = HostStatus.Pending;
			hostProfile.IsActive = false;
			hostProfile.CreatedAt = DateTime.UtcNow;

			try
			{
				await _unitOfWork.BeginTransactionAsync();
				// Add host profile to repository
				await _hostProfileRepository.AddAsync(hostProfile);
				await _unitOfWork.SaveChangesAsync();

				// Upload ID card images to Cloudinary
				if (dto.IdentityCardFrontFile != null)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(
						new ImageUploadDto
						{
							File = dto.IdentityCardFrontFile,
							Folder = $"{FolderImages.Hosts}/{hostProfile.Id}"
						}
					);
					if (!uploadResult.Success)
					{
						_logger.LogError($"Failed to upload front ID card image: {uploadResult.ErrorMessage}");
						throw new Exception($"Failed to upload front ID card image: {uploadResult.ErrorMessage}");
					}
					hostProfile.IdentityCardFrontUrl = uploadResult.Data.Url;
					uploadedImages.Add(uploadResult.Data.PublicId);
					_logger.LogInformation($"Front ID card image uploaded successfully for host ID {hostProfile.Id}");
				}

				// Upload back side of ID card
				if (dto.IdentityCardBackFile != null)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(
						new ImageUploadDto
						{
							File = dto.IdentityCardBackFile,
							Folder = $"{FolderImages.Hosts}/{hostProfile.Id}"
						}
					);
					if (!uploadResult.Success)
					{
						_logger.LogError($"Failed to upload back ID card image: {uploadResult.ErrorMessage}");
						throw new Exception($"Failed to upload back ID card image: {uploadResult.ErrorMessage}");
					}
					hostProfile.IdentityCardBackUrl = uploadResult.Data.Url;
					uploadedImages.Add(uploadResult.Data.PublicId);
					_logger.LogInformation($"Back ID card image uploaded successfully for host ID {hostProfile.Id}");
				}

				// Upload business license if provided
				if (dto.BusinessLicenseFile != null)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(
						new ImageUploadDto
						{
							File = dto.BusinessLicenseFile,
							Folder = $"{FolderImages.Lisenses}/{hostProfile.Id}"
						}
					);
					if (!uploadResult.Success)
					{
						_logger.LogError($"Failed to upload business license image: {uploadResult.ErrorMessage}");
						throw new Exception($"Failed to upload business license image: {uploadResult.ErrorMessage}");
					}
					hostProfile.BusinessLicenseUrl = uploadResult.Data.Url;
					uploadedImages.Add(uploadResult.Data.PublicId);
					_logger.LogInformation($"Business license image uploaded successfully for host ID {hostProfile.Id}");
				}

				// Upload business license if provided
				if (dto.TaxCodeDocumentFile != null)
				{
					var uploadResult = await _cloudinaryService.UploadImageAsync(
						new ImageUploadDto
						{
							File = dto.TaxCodeDocumentFile,
							Folder = $"{FolderImages.Lisenses}/{hostProfile.Id}"
						}
					);
					if (!uploadResult.Success)
					{
						_logger.LogError($"Failed to upload tax code document image: {uploadResult.ErrorMessage}");
						throw new Exception($"Failed to upload tax code document image: {uploadResult.ErrorMessage}");
					}
					hostProfile.TaxCodeDocumentUrl = uploadResult.Data.Url;
					uploadedImages.Add(uploadResult.Data.PublicId);
					_logger.LogInformation($"Tax code document image uploaded successfully for host ID {hostProfile.Id}");
				}

				_logger.LogInformation($"Host profile created successfully for user ID {dto.UserId}");
				_hostProfileRepository.Update(hostProfile);

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while registering host profile.");
				// Rollback transaction
				await _unitOfWork.RollbackTransactionAsync();
				// Cleanup uploaded images in case of failure
				foreach (var publicId in uploadedImages)
				{
					await _cloudinaryService.DeleteImageAsync(publicId);
					_logger.LogInformation($"Rolled back uploaded image with public ID {publicId}");
				}
				throw;
			}
		}

		public async Task<bool> RemoveHostProfileAsync(int hostId, int currentUserId)
		{
			// Verify user existence
			var hostProfile = await _hostProfileRepository.GetByIdAsync(hostId);
			if (hostProfile == null)
			{
				_logger.LogWarning($"Host with ID {hostId} not found.");
				throw new NotFoundException($"Host with ID: {hostId} not found.");
			}

			var user = await _userManager.FindByIdAsync(currentUserId.ToString());
			var roles = await _userManager.GetRolesAsync(user);
			var isHost = roles.Any(r => r.Equals("Host", StringComparison.OrdinalIgnoreCase));
			var isAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));


			var isCurrentUser = currentUserId == user.Id;
			if (!isCurrentUser && !isHost && !isAdmin)
			{
				_logger.LogWarning("User with ID {CurrentUserId} is not authorized to update host profile {HostUserId}.", currentUserId, hostId);
				throw new UnauthorizedAccessException("You do not have permission to remove this host profile.");
			}

			try
			{
				await _unitOfWork.BeginTransactionAsync();
				// Delete associated images from Cloudinary
				var imagePublicIds = new List<string>();
				if (!string.IsNullOrEmpty(hostProfile.IdentityCardFrontUrl))
				{
					imagePublicIds.Add(_cloudinaryService.GetPublicIdFromUrl(hostProfile.IdentityCardFrontUrl));
				}
				if (!string.IsNullOrEmpty(hostProfile.IdentityCardBackUrl))
				{
					imagePublicIds.Add(_cloudinaryService.GetPublicIdFromUrl(hostProfile.IdentityCardBackUrl));
				}
				if (!string.IsNullOrEmpty(hostProfile.BusinessLicenseUrl))
				{
					imagePublicIds.Add(_cloudinaryService.GetPublicIdFromUrl(hostProfile.BusinessLicenseUrl));
				}
				foreach (var publicId in imagePublicIds)
				{
					var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
					if (deleteResult.Success)
					{
						_logger.LogInformation($"Deleted image with public ID {publicId} from Cloudinary.");
					}
					else
					{
						_logger.LogWarning($"Failed to delete image with public ID {publicId} from Cloudinary: {deleteResult.ErrorMessage}");
						throw new Exception($"Failed to delete image with public ID {publicId} from Cloudinary: {deleteResult.ErrorMessage}");
					}
				}
				// Soft Remove host profile from repository

				hostProfile.IsActive = false;
				hostProfile.Status = HostStatus.Cancelled;
				hostProfile.UpdatedAt = DateTime.UtcNow;
				hostProfile.IsDeleted = true;
				hostProfile.IdentityCardBackUrl = null;
				hostProfile.IdentityCardFrontUrl = null;
				hostProfile.BusinessLicenseUrl = null;
				_hostProfileRepository.Update(hostProfile);

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();
				_logger.LogInformation($"Host profile for user ID {hostId} removed successfully.");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error occurred while removing host profile for user ID {hostId}.");
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}


	}
}
