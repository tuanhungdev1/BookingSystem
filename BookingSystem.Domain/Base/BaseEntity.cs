using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingSystem.Domain.Base
{
	public abstract class BaseEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		public bool IsDeleted { get; set; } = false;

		public DateTime? DeletedAt { get; set; }

		[MaxLength(100)]
		public string? CreatedBy { get; set; }

		[MaxLength(100)]
		public string? UpdatedBy { get; set; }

		[MaxLength(100)]
		public string? DeletedBy { get; set; }
	}
}
