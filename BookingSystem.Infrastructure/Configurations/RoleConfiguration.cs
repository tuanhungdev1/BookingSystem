using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class RoleConfiguration : IEntityTypeConfiguration<Role>
	{
		public void Configure(EntityTypeBuilder<Role> builder)
		{
			builder.ToTable("Roles");

			builder.Property(r => r.Description)
				.HasMaxLength(255);

			builder.HasIndex(r => r.Name)
				.IsUnique()
				.HasDatabaseName("IX_Roles_Name");
		}
	}
}
