using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class UserPreference : BaseEntity
	{
		public string PreferenceKey { get; set; } = string.Empty;

		public string PreferenceValue { get; set; } = string.Empty;

		public string? DataType { get; set; } = "string";

		public int UserId { get; set; } 

		public virtual User User { get; set; } = null!;
	}
}
