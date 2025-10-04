using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.ToTable("Users");

			builder.Property(u => u.FirstName)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(u => u.LastName)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(u => u.Address)
				.HasMaxLength(255);

			builder.Property(u => u.City)
				.HasMaxLength(100);

			builder.Property(u => u.Country)
				.HasMaxLength(100);

			builder.Property(u => u.PostalCode)
				.HasMaxLength(20);

			builder.Property(u => u.Avatar)
				.HasMaxLength(500);

			builder.Property(u => u.Gender)
				.HasConversion<int>();

			builder.HasIndex(u => u.Email)
				.IsUnique()
				.HasDatabaseName("IX_Users_Email");

			builder.HasIndex(u => u.PhoneNumber)
				.HasDatabaseName("IX_Users_PhoneNumber");

			builder.HasIndex(u => new { u.FirstName, u.LastName })
				.HasDatabaseName("IX_Users_FullName");

			// Relationships
			builder.HasOne(u => u.HostProfile)
				.WithOne(hp => hp.User)
				.HasForeignKey<HostProfile>(hp => hp.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
