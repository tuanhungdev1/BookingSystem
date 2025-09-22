using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Infrastructure.Configurations
{
	public class HotelAmenityConfiguration : IEntityTypeConfiguration<AccommodationAmenity>
	{
		public void Configure(EntityTypeBuilder<AccommodationAmenity> builder)
		{
			builder.ToTable("HotelAmenities");

			builder.HasKey(ha => new { ha.HotelId, ha.AmenityId });

			builder.HasOne(ha => ha.Accommodation)
				.WithMany(h => h.HotelAmenities)
				.HasForeignKey(ha => ha.HotelId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(ha => ha.Amenity)
				.WithMany(a => a.HotelAmenities)
				.HasForeignKey(ha => ha.AmenityId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
