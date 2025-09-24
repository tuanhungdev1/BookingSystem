using BookingSystem.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Entities
{
	public class AccommodationType : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? Image { get; set; }
		public bool IsActive { get; set; } = true;

		// Navigation Properties
		public virtual ICollection<Accommodation> Accommodations { get; set; } = new HashSet<Accommodation>();
	}
}
