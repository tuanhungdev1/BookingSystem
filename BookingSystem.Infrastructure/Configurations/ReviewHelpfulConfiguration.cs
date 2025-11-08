using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Infrastructure.Configurations
{
	public class ReviewHelpfulConfiguration : IEntityTypeConfiguration<ReviewHelpful>
	{
		public void Configure(EntityTypeBuilder<ReviewHelpful> builder)
		{
			// Đặt tên bảng
			builder.ToTable("ReviewHelpfuls");

			// Khóa chính
			builder.HasKey(rh => rh.Id);

			// Cấu hình thuộc tính
			builder.Property(rh => rh.UserId)
				.IsRequired();

			builder.Property(rh => rh.ReviewId)
				.IsRequired();

			// Quan hệ với User
			builder.HasOne(rh => rh.User)
				.WithMany(u => u.ReviewHelpfuls) // cần có ICollection<ReviewHelpful> ReviewHelpfuls trong User
				.HasForeignKey(rh => rh.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Quan hệ với Review
			builder.HasOne(rh => rh.Review)
				.WithMany(r => r.ReviewHelpfuls) // cần có ICollection<ReviewHelpful> ReviewHelpfuls trong Review
				.HasForeignKey(rh => rh.ReviewId)
				.OnDelete(DeleteBehavior.Cascade);

			// Unique constraint đảm bảo 1 user chỉ có thể "đánh dấu hữu ích" 1 lần cho 1 review
			builder.HasIndex(rh => new { rh.UserId, rh.ReviewId })
				.IsUnique();

			// Query filter bỏ qua các bản ghi đã xóa (nếu kế thừa BaseEntity có IsDeleted)
			builder.HasQueryFilter(rh => !rh.IsDeleted);
		}
	}
}
