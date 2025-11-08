using BookingSystem.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Domain.Entities
{
	public class Review : BaseEntity
	{
		public int OverallRating { get; set; }

		public int CleanlinessRating { get; set; }

		public int LocationRating { get; set; }

		public int ValueForMoneyRating { get; set; }

		public int CommunicationRating { get; set; }

		public int CheckInRating { get; set; }

		[MaxLength(2000)]
		public string? ReviewComment { get; set; }

		public bool IsVisible { get; set; } = true;

		public bool IsRecommended { get; set; } = true;

		public int HelpfulCount { get; set; } = 0;

		public string? HostResponse { get; set; }

		public DateTime? HostRespondedAt { get; set; }

		public int ReviewerId { get; set; } 

		public int RevieweeId { get; set; }

		public int BookingId { get; set; }

		public int HomestayId { get; set; }

		// Navigation Properties
		public virtual User Reviewer { get; set; } = null!;
		public virtual User Reviewee { get; set; } = null!;
		public virtual Booking Booking { get; set; } = null!;
		public virtual Homestay Homestay { get; set; } = null!;
		public ICollection<ReviewHelpful> ReviewHelpfuls { get; set; } = new List<ReviewHelpful>();
	}
}
