using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class BookingConfiguration : IEntityTypeConfiguration<Booking>
	{
		public void Configure(EntityTypeBuilder<Booking> builder)
		{
			builder.ToTable("Bookings");

			// Properties Configuration
			builder.Property(b => b.BookingCode)
				.IsRequired()
				.HasMaxLength(20);

			builder.Property(b => b.BaseAmount)
				.HasPrecision(18, 2);

			builder.Property(b => b.CleaningFee)
				.HasPrecision(18, 2)
				.HasDefaultValue(0);

			builder.Property(b => b.ServiceFee)
				.HasPrecision(18, 2)
				.HasDefaultValue(0);

			builder.Property(b => b.TaxAmount)
				.HasPrecision(18, 2)
				.HasDefaultValue(0);

			builder.Property(b => b.DiscountAmount)
				.HasPrecision(18, 2)
				.HasDefaultValue(0);

			builder.Property(b => b.TotalAmount)
				.HasPrecision(18, 2);

			builder.Property(b => b.SpecialRequests)
				.HasMaxLength(1000);

			builder.Property(b => b.CancellationReason)
				.HasMaxLength(1000);

			builder.Property(b => b.CancelledBy)
				.HasMaxLength(100);

			// Relationships
			builder.HasOne(b => b.Guest)
				.WithMany(u => u.GuestBookings)
				.HasForeignKey(b => b.GuestId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(b => b.Homestay)
				.WithMany(h => h.Bookings)
				.HasForeignKey(b => b.HomestayId)
				.OnDelete(DeleteBehavior.Restrict);

			// Indexes
			builder.HasIndex(b => b.BookingCode)
				.IsUnique();

			builder.HasIndex(b => new { b.CheckInDate, b.CheckOutDate });
			builder.HasIndex(b => b.BookingStatus);
			builder.HasIndex(b => new { b.GuestId, b.BookingStatus });
			builder.HasIndex(b => new { b.HomestayId, b.CheckInDate, b.CheckOutDate });

			// Query Filters
			builder.HasQueryFilter(b => !b.IsDeleted);
		}
	}
}
