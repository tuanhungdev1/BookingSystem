using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class HomestayRuleConfiguration : IEntityTypeConfiguration<HomestayRule>
	{
		public void Configure(EntityTypeBuilder<HomestayRule> builder)
		{
			builder.ToTable("HomestayRules");

			// Composite Primary Key
			builder.HasKey(hr => new { hr.HomestayId, hr.RuleId });

			// Properties Configuration
			builder.Property(hr => hr.CustomNote)
				.HasMaxLength(500);

			builder.Property(hr => hr.AssignedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasOne(hr => hr.Homestay)
				.WithMany(h => h.HomestayRules)
				.HasForeignKey(hr => hr.HomestayId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(hr => hr.Rule)
				.WithMany(r => r.HomestayRules)
				.HasForeignKey(hr => hr.RuleId)
				.OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(hr => hr.AssignedAt);
		}
	}
}
