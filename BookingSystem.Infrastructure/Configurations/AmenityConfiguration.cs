using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
	{
		public void Configure(EntityTypeBuilder<Amenity> builder)
		{
			builder.ToTable("Amenities");

			// Properties Configuration
			builder.Property(a => a.AmenityName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(a => a.AmenityDescription)
				.HasMaxLength(500);

			builder.Property(a => a.IconUrl)
				.HasMaxLength(200);

			builder.Property(a => a.Category)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(a => a.IsActive)
				.HasDefaultValue(true);

			builder.Property(a => a.DisplayOrder)
				.HasDefaultValue(0);

			// Indexes
			builder.HasIndex(a => a.Category);
			builder.HasIndex(a => new { a.Category, a.DisplayOrder });

			// Query Filters
			builder.HasQueryFilter(a => !a.IsDeleted);
		}
	}
}
