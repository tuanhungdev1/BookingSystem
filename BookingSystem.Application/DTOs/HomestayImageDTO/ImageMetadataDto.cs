namespace BookingSystem.Application.DTOs.HomestayImageDTO
{
	public class ImageMetadataDto
	{
		public int ImageId { get; set; }
		public string? ImageTitle { get; set; }
		public string? ImageDescription { get; set; }
		public int? DisplayOrder { get; set; }
		public bool? IsPrimaryImage { get; set; }
		public string? RoomType { get; set; }
	}
}
