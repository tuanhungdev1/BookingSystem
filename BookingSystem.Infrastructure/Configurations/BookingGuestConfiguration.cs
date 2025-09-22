using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class BookingGuestConfiguration : IEntityTypeConfiguration<BookingGuest>
	{
		public void Configure(EntityTypeBuilder<BookingGuest> entity)
		{
			entity.ToTable("BookingGuests");

			entity.Property(bg => bg.FirstName)
				.IsRequired()
				.HasMaxLength(50);

			entity.Property(bg => bg.LastName)
				.IsRequired()
				.HasMaxLength(50);

			entity.Property(bg => bg.Gender)
				.HasConversion<int>();

			entity.Property(bg => bg.IdentityNumber)
				.HasMaxLength(50);

			entity.Property(bg => bg.IdentityType)
				.HasMaxLength(50);

			entity.Property(bg => bg.Nationality)
				.HasMaxLength(50);

			entity.HasOne(bg => bg.Booking)
				.WithMany(b => b.BookingGuests)
				.HasForeignKey(bg => bg.BookingId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(bg => bg.IdentityNumber)
				.HasDatabaseName("IX_BookingGuests_IdentityNumber");
		}
	}
}
