using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class RoomTypeAmenityConfiguration : IEntityTypeConfiguration<RoomTypeAmenity>
	{
		public void Configure(EntityTypeBuilder<RoomTypeAmenity> entity)
		{
			entity.ToTable("RoomTypeAmenities");
			entity.HasKey(rta => new { rta.RoomTypeId, rta.AmenityId });

			entity.HasOne(rta => rta.RoomType)
				.WithMany(rt => rt.RoomTypeAmenities)
				.HasForeignKey(rta => rta.RoomTypeId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(rta => rta.Amenity)
				.WithMany(a => a.RoomTypeAmenities)
				.HasForeignKey(rta => rta.AmenityId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
