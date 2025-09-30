using BookingSystem.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.Services
{
	public class ImageValidationService : IImageValidationService
	{
		private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
		private readonly int _maxFileSizeInMB = 5;
		private readonly int _maxFiles = 20;

		public (bool IsValid, string ErrorMessage) ValidateImage(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return (false, "File cannot be empty");

			// Check file size
			var maxSizeInBytes = _maxFileSizeInMB * 1024 * 1024;
			if (file.Length > maxSizeInBytes)
				return (false, $"File is too large. Maximum allowed size is {_maxFileSizeInMB}MB");

			// Check extension
			var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (!_allowedExtensions.Contains(extension))
				return (false, $"Unsupported file format. Supported formats: {string.Join(", ", _allowedExtensions)}");

			// Check MIME type
			if (!IsValidImageMimeType(file.ContentType))
				return (false, "Invalid image MIME type");

			return (true, string.Empty);
		}

		public (bool IsValid, string ErrorMessage) ValidateMultipleImages(List<IFormFile> files)
		{
			if (files == null || !files.Any())
				return (false, "File list cannot be empty");

			if (files.Count > _maxFiles)
				return (false, $"You can upload up to {_maxFiles} files only");

			for (int i = 0; i < files.Count; i++)
			{
				var (isValid, errorMessage) = ValidateImage(files[i]);
				if (!isValid)
					return (false, $"File {i + 1}: {errorMessage}");
			}

			return (true, string.Empty);
		}

		private bool IsValidImageMimeType(string contentType)
		{
			var validMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
			return validMimeTypes.Contains(contentType?.ToLowerInvariant());
		}
	}
}
