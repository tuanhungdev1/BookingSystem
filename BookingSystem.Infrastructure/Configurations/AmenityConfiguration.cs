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

			builder.Property(a => a.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(a => a.Description)
				.HasMaxLength(500);

			builder.Property(a => a.Icon)
				.HasMaxLength(100);

			builder.Property(a => a.Category)
				.HasMaxLength(50);

			builder.HasIndex(a => a.Name)
				.IsUnique()
				.HasDatabaseName("IX_Amenities_Name");

			builder.HasIndex(a => a.Category)
				.HasDatabaseName("IX_Amenities_Category");
		}
	}
}
