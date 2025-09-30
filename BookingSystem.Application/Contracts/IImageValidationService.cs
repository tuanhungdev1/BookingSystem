using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.Contracts
{
	public interface IImageValidationService
	{
		(bool IsValid, string ErrorMessage) ValidateImage(IFormFile file);
		(bool IsValid, string ErrorMessage) ValidateMultipleImages(List<IFormFile> files);
	}
}
