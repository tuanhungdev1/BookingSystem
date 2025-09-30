using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Infrastructure.Configurations
{
	public class HomestayAmenityConfiguration : IEntityTypeConfiguration<HomestayAmenity>
	{
		public void Configure(EntityTypeBuilder<HomestayAmenity> builder)
		{
			builder.ToTable("HomestayAmenities");

			// Composite Primary Key
			builder.HasKey(ha => new { ha.HomestayId, ha.AmenityId });

			// Properties
			builder.Property(ha => ha.AssignedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasOne(ha => ha.Homestay)
				.WithMany(h => h.HomestayAmenities)
				.HasForeignKey(ha => ha.HomestayId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(ha => ha.Amenity)
				.WithMany(a => a.HomestayAmenities)
				.HasForeignKey(ha => ha.AmenityId)
				.OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(ha => ha.AssignedAt);
		}
	}
}
