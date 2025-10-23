namespace BookingSystem.Application.DTOs.HomestayDTO
{
	public class HomestayImageDto
	{
		public int Id { get; set; }

		public string ImageUrl { get; set; } = string.Empty;

		public string? ImageTitle { get; set; }

		public string? ImageDescription { get; set; }

		public int DisplayOrder { get; set; }

		public bool IsPrimaryImage { get; set; }

		public string? RoomType { get; set; }

		public int HomestayId { get; set; }

		public DateTime CreatedAt { get; set; } 

		public DateTime? UpdatedAt { get; set; }

		public bool IsDeleted { get; set; } = false;

		public DateTime? DeletedAt { get; set; }
		public string? CreatedBy { get; set; }
		public string? UpdatedBy { get; set; }
		public string? DeletedBy { get; set; }
	}
}
