using BookingSystem.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Domain.Entities
{
	public class PropertyType : BaseEntity
	{
		[Required]
		[MaxLength(100)]
		public string TypeName { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? Description { get; set; }

		public string? IconUrl { get; set; }

		public bool IsActive { get; set; } = true;

		public int DisplayOrder { get; set; } = 0;

		// Navigation Properties
		public virtual ICollection<Homestay> Homestays { get; set; } = new List<Homestay>();
	}
}
