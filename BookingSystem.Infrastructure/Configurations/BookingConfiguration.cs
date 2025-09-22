using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class BookingConfiguration : IEntityTypeConfiguration<Booking>
	{
		public void Configure(EntityTypeBuilder<Booking> builder)
		{
			builder.ToTable("Bookings");

			builder.Property(b => b.BookingNumber)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(b => b.TotalAmount)
				.HasPrecision(10, 2);

			builder.Property(b => b.TaxAmount)
				.HasPrecision(10, 2);

			builder.Property(b => b.DiscountAmount)
				.HasPrecision(10, 2);

			builder.Property(b => b.FinalAmount)
				.HasPrecision(10, 2);

			builder.Property(b => b.Status)
				.HasConversion<int>()
				.HasDefaultValue(BookingStatus.Pending);

			builder.Property(b => b.SpecialRequests)
				.HasMaxLength(1000);

			builder.Property(b => b.GuestFirstName)
				.HasMaxLength(50);

			builder.Property(b => b.GuestLastName)
				.HasMaxLength(50);

			builder.Property(b => b.GuestEmail)
				.HasMaxLength(100);

			builder.Property(b => b.GuestPhone)
				.HasMaxLength(20);

			builder.HasOne(b => b.User)
				.WithMany(u => u.Bookings)
				.HasForeignKey(b => b.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(b => b.RoomType)
				.WithMany(rt => rt.Bookings)
				.HasForeignKey(b => b.RoomTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(b => b.BookingNumber)
				.IsUnique()
				.HasDatabaseName("IX_Bookings_BookingNumber");

			builder.HasIndex(b => new { b.CheckInDate, b.CheckOutDate })
				.HasDatabaseName("IX_Bookings_CheckInOut_Dates");

			builder.HasIndex(b => b.Status)
				.HasDatabaseName("IX_Bookings_Status");

			builder.HasIndex(b => b.UserId)
				.HasDatabaseName("IX_Bookings_UserId");
		}
	}
}
