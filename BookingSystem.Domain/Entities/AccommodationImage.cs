using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class AccommodationImage : BaseEntity
	{
		public string ImageUrl { get; set; } = string.Empty;
		public string? Caption { get; set; }
		public int Order { get; set; }
		public bool IsMain { get; set; }
		public Guid HotelId { get; set; }

		// Navigation Properties
		public virtual Accommodation Accommodation { get; set; } = null!;
	}
}
