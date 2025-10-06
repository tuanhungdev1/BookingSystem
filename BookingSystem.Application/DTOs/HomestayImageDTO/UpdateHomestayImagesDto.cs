namespace BookingSystem.Application.DTOs.HomestayImageDTO
{
	public class UpdateHomestayImagesDto
	{
		// Danh sách ID ảnh muốn GIỮ LẠI
		public List<int> KeepImageIds { get; set; } = new();

		public List<CreateHomestayImageDto> NewImages { get; set; } = new();

		// (Optional) Cập nhật thông tin ảnh cũ
		public List<ImageMetadataDto>? UpdateExistingImages { get; set; }
	}
}
