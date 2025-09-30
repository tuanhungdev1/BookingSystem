namespace BookingSystem.Application.DTOs.AmenityDTO
{
	public class AmenityDto
	{
		public int Id { get; set; }
		public string AmenityName { get; set; } = string.Empty;
		public string? AmenityDescription { get; set; }
		public string? IconUrl { get; set; }
		public string Category { get; set; } = string.Empty;
		public int UsageCount { get; set; }
		public bool IsActive { get; set; }
		public int DisplayOrder { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
