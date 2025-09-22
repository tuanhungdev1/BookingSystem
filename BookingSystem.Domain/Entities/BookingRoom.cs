namespace BookingSystem.Domain.Entities
{
	public class BookingRoom
	{
		public Guid BookingId { get; set; }
		public Guid RoomId { get; set; }
		public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
		public string? Notes { get; set; }

		// Navigation Properties
		public virtual Booking Booking { get; set; } = null!;
		public virtual Room Room { get; set; } = null!;
	}
}
