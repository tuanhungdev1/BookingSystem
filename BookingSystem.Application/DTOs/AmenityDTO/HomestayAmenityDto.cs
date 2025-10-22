using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.AmenityDTO
{
	public class HomestayAmenityDto
	{
		public int HomestayId { get; set; }
		public int AmenityId { get; set; }
		public DateTime AssignedAt { get; set; }
		public string? CustomNote { get; set; }
		public bool IsHighlight { get; set; }
		public AmenityDto Amenity { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
