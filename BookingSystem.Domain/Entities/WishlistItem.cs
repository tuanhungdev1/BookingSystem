using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
	public class WishlistItem : BaseEntity
	{
		public string? PersonalNote { get; set; }

		public int UserId { get; set; }

		public int HomestayId { get; set; }

		public virtual User User { get; set; } = null!;
		public virtual Homestay Homestay { get; set; } = null!;
	}
}
