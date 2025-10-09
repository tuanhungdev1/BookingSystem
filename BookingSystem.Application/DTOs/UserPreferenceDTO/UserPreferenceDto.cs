using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.UserPreferenceDTO
{
	public class UserPreferenceDto
	{
		public int Id { get; set; }
		public string PreferenceKey { get; set; } = string.Empty;
		public string PreferenceValue { get; set; } = string.Empty;
		public string? DataType { get; set; }
		public int UserId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
