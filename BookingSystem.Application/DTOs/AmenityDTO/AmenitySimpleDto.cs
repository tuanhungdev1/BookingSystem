namespace BookingSystem.Application.DTOs.AmenityDTO
{
	public class AmenitySimpleDto
	{
		public int Id { get; set; }
		public string AmenityName { get; set; } = string.Empty;
		public string? AmenityDescription { get; set; }
		public string? IconUrl { get; set; }
		public string Category { get; set; } = string.Empty;
		public int DisplayOrder { get; set; }

		// Thông tin từ bảng trung gian (nếu cần)
		public string? CustomNote { get; set; }
		public bool IsHighlight { get; set; }
	}
}
