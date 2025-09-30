using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class PropertyTypeConfiguration : IEntityTypeConfiguration<PropertyType>
	{
		public void Configure(EntityTypeBuilder<PropertyType> builder)
		{
			builder.ToTable("PropertyTypes");

			// Properties Configuration
			builder.Property(pt => pt.TypeName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(pt => pt.Description)
				.HasMaxLength(500);

			builder.Property(pt => pt.IconUrl)
				.HasMaxLength(200);

			builder.Property(pt => pt.IsActive)
				.HasDefaultValue(true);

			builder.Property(pt => pt.DisplayOrder)
				.HasDefaultValue(0);

			// Indexes
			builder.HasIndex(pt => pt.TypeName)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			builder.HasIndex(pt => new { pt.IsActive, pt.DisplayOrder });

			// Query Filters
			builder.HasQueryFilter(pt => !pt.IsDeleted);
		}
	}
}
