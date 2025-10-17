using BookingSystem.Application.Contracts;
using BookingSystem.Application.DTOs.ImageDTO;
using BookingSystem.Application.Models.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Net;

namespace BookingSystem.Application.Services
{
	public class CloudinaryService : ICloudinaryService
	{
		private readonly Cloudinary _cloudinary;
		private readonly IImageValidationService _validationService;
		private readonly ILogger<CloudinaryService> _logger;
		private readonly CloudinarySettings _cloudinarySettings;

		public CloudinaryService(
			IOptions<CloudinarySettings> config,
			IImageValidationService validationService,
			ILogger<CloudinaryService> logger)
		{
			_cloudinarySettings = config.Value;
			var account = new Account(
				config.Value.CloudName,
				config.Value.ApiKey,
				config.Value.ApiSecret);

			_cloudinary = new Cloudinary(account);
			_validationService = validationService;
			_logger = logger;
		}

		public async Task<UploadImageResult<ImageResponseDto>> UploadImageAsync(ImageUploadDto uploadDto)
		{
			try
			{
				var (isValid, errorMessage) = _validationService.ValidateImage(uploadDto.File);
				if (!isValid)
				{
					return new UploadImageResult<ImageResponseDto>
					{
						Success = false,
						ErrorMessage = errorMessage
					};
				}

				var uploadParams = new ImageUploadParams()
				{
					File = new FileDescription(uploadDto.File.FileName, uploadDto.File.OpenReadStream()),
					PublicId = Guid.NewGuid().ToString(),
					Folder = uploadDto.Folder ?? "uploads",
					Overwrite = false
				};

				var uploadResult = await _cloudinary.UploadAsync(uploadParams);

				if (uploadResult.StatusCode == HttpStatusCode.OK)
				{
					return new UploadImageResult<ImageResponseDto>
					{
						Success = true,
						Data = new ImageResponseDto
						{
							PublicId = uploadResult.PublicId,
							Url = uploadResult.Uri?.ToString() ?? "",
							SecureUrl = uploadResult.SecureUri?.ToString() ?? ""
						}
					};
				}

				return new UploadImageResult<ImageResponseDto>
				{
					Success = false,
					ErrorMessage = uploadResult.Error?.Message ?? "Upload thất bại"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi upload ảnh");
				return new UploadImageResult<ImageResponseDto>
				{
					Success = false,
					ErrorMessage = "Có lỗi xảy ra khi upload ảnh"
				};
			}
		}

		public async Task<UploadImageResult<List<ImageResponseDto>>> UploadMultipleImagesAsync(MultipleImageUploadDto uploadDto)
		{
			var (isValid, errorMessage) = _validationService.ValidateMultipleImages(uploadDto.Files);
			if (!isValid)
			{
				return new UploadImageResult<List<ImageResponseDto>>
				{
					Success = false,
					ErrorMessage = errorMessage
				};
			}

			var uploadedImages = new List<ImageResponseDto>();
			var uploadedPublicIds = new List<string>();

			try
			{
				foreach (var file in uploadDto.Files)
				{
					var singleUploadDto = new ImageUploadDto
					{
						File = file,
						Folder = uploadDto.Folder
					};

					var result = await UploadImageAsync(singleUploadDto);

					if (result.Success && result.Data != null)
					{
						uploadedImages.Add(result.Data);
						uploadedPublicIds.Add(result.Data.PublicId);
					}
					else
					{
						await RollbackAsync(uploadedPublicIds);
						return new UploadImageResult<List<ImageResponseDto>>
						{
							Success = false,
							ErrorMessage = $"Upload thất bại tại file: {file.FileName}"
						};
					}
				}

				return new UploadImageResult<List<ImageResponseDto>>
				{
					Success = true,
					Data = uploadedImages
				};
			}
			catch (Exception ex)
			{
				await RollbackAsync(uploadedPublicIds);
				_logger.LogError(ex, "Lỗi khi upload nhiều ảnh");
				return new UploadImageResult<List<ImageResponseDto>>
				{
					Success = false,
					ErrorMessage = "Có lỗi xảy ra khi upload nhiều ảnh"
				};
			}
		}

		public async Task<UploadImageResult<bool>> DeleteImageAsync(string publicId)
		{
			try
			{
				var deleteParams = new DeletionParams(publicId)
				{
					ResourceType = ResourceType.Image
				};

				var result = await _cloudinary.DestroyAsync(deleteParams);

				if (result.StatusCode == HttpStatusCode.OK && result.Result == "ok")
				{
					return new UploadImageResult<bool> { Success = true, Data = true };
				}

				return new UploadImageResult<bool>
				{
					Success = false,
					ErrorMessage = "Xóa ảnh thất bại"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Lỗi khi xóa ảnh {publicId}");
				return new UploadImageResult<bool>
				{
					Success = false,
					ErrorMessage = "Có lỗi xảy ra khi xóa ảnh"
				};
			}
		}

		public async Task<UploadImageResult<bool>> DeleteMultipleImagesAsync(List<string> publicIds)
		{
			try
			{
				var deleteParams = new DelResParams()
				{
					PublicIds = publicIds,
					ResourceType = ResourceType.Image
				};

				await _cloudinary.DeleteResourcesAsync(deleteParams);
				return new UploadImageResult<bool> { Success = true, Data = true };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa nhiều ảnh");
				return new UploadImageResult<bool>
				{
					Success = false,
					ErrorMessage = "Có lỗi xảy ra khi xóa nhiều ảnh"
				};
			}
		}

		public async Task<UploadImageResult<ImageResponseDto>> ReplaceImageAsync(ImageReplaceDto replaceDto)
		{
			try
			{
				var (isValid, errorMessage) = _validationService.ValidateImage(replaceDto.NewFile);
				if (!isValid)
				{
					return new UploadImageResult<ImageResponseDto>
					{
						Success = false,
						ErrorMessage = errorMessage
					};
				}

				// Upload file mới
				var uploadDto = new ImageUploadDto
				{
					File = replaceDto.NewFile,
					Folder = replaceDto.Folder
				};

				var uploadResult = await UploadImageAsync(uploadDto);

				if (!uploadResult.Success)
				{
					return uploadResult;
				}

				// Xóa file cũ (không rollback nếu thất bại, chỉ log warning)
				var deleteResult = await DeleteImageAsync(replaceDto.OldPublicId);
				if (!deleteResult.Success)
				{
					_logger.LogWarning($"Không thể xóa file cũ {replaceDto.OldPublicId}");
				}

				return uploadResult;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Lỗi khi thay thế ảnh {replaceDto.OldPublicId}");
				return new UploadImageResult<ImageResponseDto>
				{
					Success = false,
					ErrorMessage = "Có lỗi xảy ra khi thay thế ảnh"
				};
			}
		}

		public string GetPublicIdFromUrl(string imageUrl)
		{
			try
			{
				var uri = new Uri(imageUrl);
				var segments = uri.AbsolutePath.Split('/');
				var uploadIndex = Array.IndexOf(segments, "upload");
				if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length)
					return null;

				var startIndex = uploadIndex + 2;
				var publicId = string.Join("/", segments.Skip(startIndex).Take(segments.Length - startIndex)).Replace(Path.GetExtension(segments.Last()), "");

				return publicId;
			}
			catch (Exception ex)
			{
				_logger.LogError("Error converting Image URL to PUBLIC_ID");
				return null;
			}
		}
		private async Task RollbackAsync(List<string> publicIds)
		{
			try
			{
				if (publicIds.Any())
				{
					await DeleteMultipleImagesAsync(publicIds);
					_logger.LogInformation($"Đã rollback {publicIds.Count} ảnh");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi rollback ảnh");
			}
		}
	}
}
