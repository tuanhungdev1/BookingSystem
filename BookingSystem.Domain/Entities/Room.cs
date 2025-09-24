using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities
{
	public class Room : BaseEntity
	{
		public string RoomNumber { get; set; } = string.Empty;
		public string Floor { get; set; } = string.Empty;
		public RoomStatus Status { get; set; } = RoomStatus.Available;
		public string? Notes { get; set; }
		public Guid AccommodationId { get; set; }
		public Guid RoomTypeId { get; set; }

		// Navigation Properties
		public virtual Accommodation Accommodation { get; set; } = null!;
		public virtual RoomType RoomType { get; set; } = null!;
		public virtual ICollection<BookingRoom> BookingRooms { get; set; } = new HashSet<BookingRoom>();
	}
}
