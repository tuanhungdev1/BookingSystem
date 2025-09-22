using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class RoomConfiguration : IEntityTypeConfiguration<Room>
	{
		public void Configure(EntityTypeBuilder<Room> builder)
		{
			builder.ToTable("Rooms");

			builder.Property(r => r.RoomNumber)
				.IsRequired()
				.HasMaxLength(10);

			builder.Property(r => r.Floor)
				.HasMaxLength(10);

			builder.Property(r => r.Status)
				.HasConversion<int>()
				.HasDefaultValue(RoomStatus.Available);

			builder.Property(r => r.Notes)
				.HasMaxLength(500);

			builder.HasOne(r => r.Accommodation)
				.WithMany(h => h.Rooms)
				.HasForeignKey(r => r.HotelId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(r => r.RoomType)
				.WithMany(rt => rt.Rooms)
				.HasForeignKey(r => r.RoomTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(r => new { r.HotelId, r.RoomNumber })
				.IsUnique()
				.HasDatabaseName("IX_Rooms_HotelId_RoomNumber");

			builder.HasIndex(r => r.Status)
				.HasDatabaseName("IX_Rooms_Status");
		}
	}
}
