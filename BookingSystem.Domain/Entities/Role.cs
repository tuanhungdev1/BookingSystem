using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Domain.Entities
{
	public class Role : IdentityRole<int>
	{
		public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
	}
}
