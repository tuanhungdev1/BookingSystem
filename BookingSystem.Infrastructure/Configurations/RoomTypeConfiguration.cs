using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
	{
		public void Configure(EntityTypeBuilder<RoomType> builder)
		{
			builder.ToTable("RoomTypes");

			builder.Property(rt => rt.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(rt => rt.Description)
				.IsRequired()
				.HasMaxLength(1000);

			builder.Property(rt => rt.BasePrice)
				.HasPrecision(10, 2)
				.IsRequired();

			builder.Property(rt => rt.Size)
				.HasPrecision(8, 2);

			builder.Property(rt => rt.BedType)
				.HasMaxLength(50);

			builder.Property(rt => rt.MainImage)
				.HasMaxLength(500);

			builder.HasOne(rt => rt.Accommodation)
				.WithMany(h => h.RoomTypes)
				.HasForeignKey(rt => rt.HotelId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(rt => new { rt.HotelId, rt.Name })
				.IsUnique()
				.HasDatabaseName("IX_RoomTypes_HotelId_Name");

			builder.HasIndex(rt => rt.BasePrice)
				.HasDatabaseName("IX_RoomTypes_BasePrice");
		}
	}
}
