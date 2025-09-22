using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Infrastructure.Configurations
{
	public class AccommodationImageConfiguration : IEntityTypeConfiguration<AccommodationImage>
	{
		public void Configure(EntityTypeBuilder<AccommodationImage> entity)
		{
			entity.ToTable("HotelImages");

			entity.Property(hi => hi.ImageUrl)
				.IsRequired()
				.HasMaxLength(500);

			entity.Property(hi => hi.Caption)
				.HasMaxLength(200);

			entity.HasOne(hi => hi.Accommodation)
				.WithMany(h => h.HotelImages)
				.HasForeignKey(hi => hi.HotelId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(hi => new { hi.HotelId, hi.Order })
				.HasDatabaseName("IX_HotelImages_HotelId_Order");
		}
	}
}
