namespace BookingSystem.Application.DTOs.PropertyTypeDTO
{
	public class PropertyTypeDto
	{
		public int Id { get; set; }
		public string TypeName { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? IconUrl { get; set; }
		public bool IsActive { get; set; }
		public int DisplayOrder { get; set; }
		public DateTime CreatedAt { get; set; } 
		public DateTime? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public string? CreatedBy { get; set; }
		public string? UpdatedBy { get; set; }
		public string? DeletedBy { get; set; }
	}
}
