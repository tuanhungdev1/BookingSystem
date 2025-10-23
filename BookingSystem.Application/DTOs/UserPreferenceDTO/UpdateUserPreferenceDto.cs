using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.UserPreferenceDTO
{
	public class UpdateUserPreferenceDto
	{
		[Required(ErrorMessage = "Preference value is required")]
		[StringLength(1000, ErrorMessage = "Preference value cannot exceed 1000 characters")]
		public string PreferenceValue { get; set; } = string.Empty;

		[StringLength(50, ErrorMessage = "Data type cannot exceed 50 characters")]
		public string? DataType { get; set; }
	}
}
