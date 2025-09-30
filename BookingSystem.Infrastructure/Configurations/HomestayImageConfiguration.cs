using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Infrastructure.Configurations
{
	public class HomestayImageConfiguration : IEntityTypeConfiguration<HomestayImage>
	{
		public void Configure(EntityTypeBuilder<HomestayImage> builder)
		{
			builder.ToTable("HomestayImages");

			// Properties Configuration
			builder.Property(hi => hi.ImageUrl)
				.IsRequired()
				.HasMaxLength(500);

			builder.Property(hi => hi.ImageTitle)
				.HasMaxLength(200);

			builder.Property(hi => hi.ImageDescription)
				.HasMaxLength(500);

			builder.Property(hi => hi.DisplayOrder)
				.HasDefaultValue(0);

			builder.Property(hi => hi.IsPrimaryImage)
				.HasDefaultValue(false);

			builder.Property(hi => hi.RoomType)
				.HasMaxLength(50);

			// Relationships
			builder.HasOne(hi => hi.Homestay)
				.WithMany(h => h.HomestayImages)
				.HasForeignKey(hi => hi.HomestayId)
				.OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(hi => new { hi.HomestayId, hi.DisplayOrder });
			builder.HasIndex(hi => new { hi.HomestayId, hi.IsPrimaryImage });

			// Query Filters
			builder.HasQueryFilter(hi => !hi.IsDeleted);
		}
	}
}
