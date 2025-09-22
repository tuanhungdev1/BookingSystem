using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class ReviewConfiguration : IEntityTypeConfiguration<Review>
	{
		public void Configure(EntityTypeBuilder<Review> entity)
		{
			entity.ToTable("Reviews");

			entity.Property(r => r.Rating)
				.IsRequired();

			entity.Property(r => r.Comment)
				.HasMaxLength(2000);

			entity.HasOne(r => r.User)
				.WithMany(u => u.Reviews)
				.HasForeignKey(r => r.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(r => r.Booking)
				.WithOne(b => b.Review)
				.HasForeignKey<Review>(r => r.BookingId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(r => r.Rating)
				.HasDatabaseName("IX_Reviews_Rating");

			entity.HasIndex(r => r.ReviewDate)
				.HasDatabaseName("IX_Reviews_ReviewDate");
		}
	}
}
