using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class BookingRoomConfiguration : IEntityTypeConfiguration<BookingRoom>
	{
		public void Configure(EntityTypeBuilder<BookingRoom> entity)
		{
			entity.ToTable("BookingRooms");
			entity.HasKey(br => new { br.BookingId, br.RoomId });

			entity.Property(br => br.Notes)
				.HasMaxLength(500);

			entity.HasOne(br => br.Booking)
				.WithMany(b => b.BookingRooms)
				.HasForeignKey(br => br.BookingId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(br => br.Room)
				.WithMany(r => r.BookingRooms)
				.HasForeignKey(br => br.RoomId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
