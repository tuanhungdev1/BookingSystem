using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
	{
		public void Configure(EntityTypeBuilder<UserPreference> builder)
		{
			builder.ToTable("UserPreferences");

			// Properties Configuration
			builder.Property(up => up.PreferenceKey)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(up => up.PreferenceValue)
				.IsRequired()
				.HasMaxLength(1000);

			builder.Property(up => up.DataType)
				.HasMaxLength(50)
				.HasDefaultValue("string");

			// Relationships
			builder.HasOne(up => up.User)
				.WithMany(u => u.UserPreferences)
				.HasForeignKey(up => up.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Unique Constraint
			builder.HasIndex(up => new { up.UserId, up.PreferenceKey })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// Query Filters
			builder.HasQueryFilter(up => !up.IsDeleted);
		}
	}
}
