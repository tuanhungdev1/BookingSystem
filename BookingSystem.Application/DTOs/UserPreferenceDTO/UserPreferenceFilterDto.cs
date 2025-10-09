using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.UserPreferenceDTO
{
	public class UserPreferenceFilterDto
	{
		public List<string>? Keys { get; set; }
		public string? SearchTerm { get; set; }
	}
}
