using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class Review : BaseEntity
	{
		public int Rating { get; set; } // 1-5
		public string? Comment { get; set; }
		public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
		public bool IsRecommended { get; set; }
		public Guid UserId { get; set; }
		public Guid BookingId { get; set; }

		// Navigation Properties
		public virtual User User { get; set; } = null!;
		public virtual Booking Booking { get; set; } = null!;
	}
}
