namespace BookingSystem.Application.DTOs.UserPreferenceDTO
{
	public class UserPreferenceFilterDto
	{
		public List<string>? Keys { get; set; }
		public string? SearchTerm { get; set; }
	}
}
