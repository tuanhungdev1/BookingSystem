using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.UserPreferenceDTO
{
	public class BulkUserPreferenceDto
	{
		[Required(ErrorMessage = "Preferences are required")]
		public Dictionary<string, string> Preferences { get; set; } = new();
	}

}
