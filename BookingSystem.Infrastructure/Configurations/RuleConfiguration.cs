using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Infrastructure.Configurations
{
	public class RuleConfiguration : IEntityTypeConfiguration<Rule>
	{
		public void Configure(EntityTypeBuilder<Rule> builder)
		{
			builder.ToTable("Rules");

			// Properties Configuration
			builder.Property(r => r.RuleName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(r => r.RuleDescription)
				.HasMaxLength(500);

			builder.Property(r => r.IconUrl)
				.HasMaxLength(200);

			builder.Property(r => r.RuleType)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(r => r.IsActive)
				.HasDefaultValue(true);

			builder.Property(r => r.DisplayOrder)
				.HasDefaultValue(0);

			// Indexes
			builder.HasIndex(r => r.RuleType);
			builder.HasIndex(r => new { r.IsActive, r.DisplayOrder });

			// Query Filters
			builder.HasQueryFilter(r => !r.IsDeleted);
		}
	}
}
