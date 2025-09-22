using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class RoomTypeImage : BaseEntity
	{
		public string ImageUrl { get; set; } = string.Empty;
		public string? Caption { get; set; }
		public int Order { get; set; }
		public bool IsMain { get; set; }
		public Guid RoomTypeId { get; set; }

		// Navigation Properties
		public virtual RoomType RoomType { get; set; } = null!;
	}
}
