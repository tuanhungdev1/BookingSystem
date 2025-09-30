using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class AvailabilityCalendar : BaseEntity
	{
		public DateOnly AvailableDate { get; set; }

		public bool IsAvailable { get; set; } = true;

		public decimal? CustomPrice { get; set; }

		public int? MinimumNights { get; set; }

		public bool IsBlocked { get; set; } = false;

		public string? BlockReason { get; set; }

		// Foreign Key
		public int HomestayId { get; set; }

		// Navigation Property
		public virtual Homestay Homestay { get; set; } = null!;
	}
}
