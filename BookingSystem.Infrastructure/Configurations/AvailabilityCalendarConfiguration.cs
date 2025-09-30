using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class AvailabilityCalendarConfiguration : IEntityTypeConfiguration<AvailabilityCalendar>
	{
		public void Configure(EntityTypeBuilder<AvailabilityCalendar> builder)
		{
			builder.ToTable("AvailabilityCalendars");

			// Properties Configuration
			builder.Property(ac => ac.IsAvailable)
				.HasDefaultValue(true);

			builder.Property(ac => ac.CustomPrice)
				.HasPrecision(18, 2);

			builder.Property(ac => ac.IsBlocked)
				.HasDefaultValue(false);

			builder.Property(ac => ac.BlockReason)
				.HasMaxLength(500);

			// Relationships
			builder.HasOne(ac => ac.Homestay)
				.WithMany(h => h.AvailabilityCalendars)
				.HasForeignKey(ac => ac.HomestayId)
				.OnDelete(DeleteBehavior.Cascade);

			// Unique Constraint
			builder.HasIndex(ac => new { ac.HomestayId, ac.AvailableDate })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// Additional Indexes
			builder.HasIndex(ac => new { ac.AvailableDate, ac.IsAvailable });
			builder.HasIndex(ac => ac.HomestayId);

			// Query Filters
			builder.HasQueryFilter(ac => !ac.IsDeleted);
		}
	}
}
