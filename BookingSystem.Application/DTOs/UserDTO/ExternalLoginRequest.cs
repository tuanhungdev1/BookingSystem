using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.UserDTO
{
	public class ExternalLoginRequest
	{
		public string Email { get; set; } = string.Empty;
		public string ExternalId { get; set; } = string.Empty;
		public string IdToken { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string? Avatar { get; set; }
	}
}
