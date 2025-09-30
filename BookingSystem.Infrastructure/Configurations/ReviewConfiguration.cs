using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class ReviewConfiguration : IEntityTypeConfiguration<Review>
	{
		public void Configure(EntityTypeBuilder<Review> builder)
		{
			builder.ToTable("Reviews");

			// Properties Configuration
			builder.Property(r => r.OverallRating)
				.IsRequired();

			builder.Property(r => r.ReviewComment)
			.HasMaxLength(2000);
			builder.Property(r => r.HostResponse)
			.HasMaxLength(1000);
			builder.Property(r => r.IsVisible)
			.HasDefaultValue(true);
			builder.Property(r => r.IsRecommended)
				.HasDefaultValue(true);

			builder.Property(r => r.HelpfulCount)
				.HasDefaultValue(0);

			// Relationships
			builder.HasOne(r => r.Reviewer)
				.WithMany(u => u.WrittenReviews)
				.HasForeignKey(r => r.ReviewerId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(r => r.Reviewee)
				.WithMany(u => u.ReceivedReviews)
				.HasForeignKey(r => r.RevieweeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(r => r.Booking)
				.WithMany(b => b.Reviews)
				.HasForeignKey(r => r.BookingId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(r => r.Homestay)
				.WithMany(h => h.Reviews)
				.HasForeignKey(r => r.HomestayId)
				.OnDelete(DeleteBehavior.Restrict);

			// Constraints
			builder.ToTable(t => t.HasCheckConstraint("CK_Review_OverallRating", "[OverallRating] BETWEEN 1 AND 5"));
			builder.ToTable(t => t.HasCheckConstraint("CK_Review_CleanlinessRating", "[CleanlinessRating] BETWEEN 1 AND 5"));
			builder.ToTable(t => t.HasCheckConstraint("CK_Review_LocationRating", "[LocationRating] BETWEEN 1 AND 5"));
			builder.ToTable(t => t.HasCheckConstraint("CK_Review_ValueForMoneyRating", "[ValueForMoneyRating] BETWEEN 1 AND 5"));
			builder.ToTable(t => t.HasCheckConstraint("CK_Review_CommunicationRating", "[CommunicationRating] BETWEEN 1 AND 5"));
			builder.ToTable(t => t.HasCheckConstraint("CK_Review_CheckInRating", "[CheckInRating] BETWEEN 1 AND 5"));

			// Indexes
			builder.HasIndex(r => new { r.HomestayId, r.IsVisible });
			builder.HasIndex(r => new { r.ReviewerId, r.CreatedAt });
			builder.HasIndex(r => r.OverallRating);
			builder.HasIndex(r => r.BookingId)
				.IsUnique();

			// Query Filters
			builder.HasQueryFilter(r => !r.IsDeleted);
		}
	}
}
