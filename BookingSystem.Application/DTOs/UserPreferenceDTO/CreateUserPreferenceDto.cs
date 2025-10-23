using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.UserPreferenceDTO
{
	public class CreateUserPreferenceDto
	{
		[Required(ErrorMessage = "Preference key is required")]
		[StringLength(100, MinimumLength = 1, ErrorMessage = "Preference key must be between 1 and 100 characters")]
		public string PreferenceKey { get; set; } = string.Empty;

		[Required(ErrorMessage = "Preference value is required")]
		[StringLength(1000, ErrorMessage = "Preference value cannot exceed 1000 characters")]
		public string PreferenceValue { get; set; } = string.Empty;

		[StringLength(50, ErrorMessage = "Data type cannot exceed 50 characters")]
		public string? DataType { get; set; } = "string";
	}
}
