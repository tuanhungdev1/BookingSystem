using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
	{
		public void Configure(EntityTypeBuilder<WishlistItem> builder)
		{
			builder.ToTable("WishlistItems");

			// Properties Configuration
			builder.Property(w => w.PersonalNote)
				.HasMaxLength(200);

			// Relationships
			builder.HasOne(w => w.User)
				.WithMany(u => u.WishlistItems)
				.HasForeignKey(w => w.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(w => w.Homestay)
				.WithMany(h => h.WishlistItems)
				.HasForeignKey(w => w.HomestayId)
				.OnDelete(DeleteBehavior.Cascade);

			// Unique Constraint
			builder.HasIndex(w => new { w.UserId, w.HomestayId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			// Query Filters
			builder.HasQueryFilter(w => !w.IsDeleted);
		}
	}
}
