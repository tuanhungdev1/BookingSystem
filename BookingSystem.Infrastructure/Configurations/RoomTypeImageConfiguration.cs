using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class RoomTypeImageConfiguration : IEntityTypeConfiguration<RoomTypeImage>
	{
		public void Configure(EntityTypeBuilder<RoomTypeImage> entity)
		{
			entity.ToTable("RoomTypeImages");

			entity.Property(rti => rti.ImageUrl)
				.IsRequired()
				.HasMaxLength(500);

			entity.Property(rti => rti.Caption)
				.HasMaxLength(200);

			entity.HasOne(rti => rti.RoomType)
				.WithMany(rt => rt.RoomTypeImages)
				.HasForeignKey(rti => rti.RoomTypeId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(rti => new { rti.RoomTypeId, rti.Order })
				.HasDatabaseName("IX_RoomTypeImages_RoomTypeId_Order");
		}
	}
}
