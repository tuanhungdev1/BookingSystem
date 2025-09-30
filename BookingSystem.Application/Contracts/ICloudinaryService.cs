using BookingSystem.Application.DTOs.ImageDTO;

namespace BookingSystem.Application.Contracts
{
	public interface ICloudinaryService
	{
		Task<UploadImageResult<ImageResponseDto>> UploadImageAsync(ImageUploadDto uploadDto);
		Task<UploadImageResult<List<ImageResponseDto>>> UploadMultipleImagesAsync(MultipleImageUploadDto uploadDto);
		Task<UploadImageResult<bool>> DeleteImageAsync(string publicId);
		Task<UploadImageResult<bool>> DeleteMultipleImagesAsync(List<string> publicIds);
		Task<UploadImageResult<ImageResponseDto>> ReplaceImageAsync(ImageReplaceDto replaceDto);
		string GetPublicIdFromUrl(string imageUrl);
	}
}
