using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
	{
		public void Configure(EntityTypeBuilder<UserRole> builder)
		{
			builder.ToTable("UserRoles");

			builder.Property(ur => ur.AssignedBy)
				.HasMaxLength(256);

			builder.HasOne(ur => ur.User)
				.WithMany(u => u.UserRoles)
				.HasForeignKey(ur => ur.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(ur => ur.Role)
				.WithMany(r => r.UserRoles)
				.HasForeignKey(ur => ur.RoleId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
