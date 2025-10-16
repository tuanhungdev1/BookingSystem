using BookingSystem.Domain.Enums;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Infrastructure.Repositories
{
	public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<int>>
	{
		public void Configure(EntityTypeBuilder<IdentityRole<int>> builder)
		{
			// Seed Role từ enum
			builder.HasData(
				Enum.GetValues(typeof(SystemRoles))
					.Cast<SystemRoles>()
					.Select((role, index) => new IdentityRole<int>
					{
						Id = index + 1,                 // Id kiểu int
						Name = role.ToString(),          // "Admin", "Host", "User"
						NormalizedName = role.ToString().ToUpper()
					})
			);
		}
	}
}
