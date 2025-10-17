using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.UserDTO
{
	public class UserAvatarDto
	{
		public int UserId { get; set; }
		public string AvatarUrl { get; set; }
		public string PublicId { get; set; }
	}
}
