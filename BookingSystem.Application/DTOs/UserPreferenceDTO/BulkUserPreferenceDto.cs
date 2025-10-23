using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.UserPreferenceDTO
{
	public class BulkUserPreferenceDto
	{
		[Required(ErrorMessage = "Preferences are required")]
		public Dictionary<string, string> Preferences { get; set; } = new();
	}

}
