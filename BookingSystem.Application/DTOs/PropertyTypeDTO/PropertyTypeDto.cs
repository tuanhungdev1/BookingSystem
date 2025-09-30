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
	}
}
