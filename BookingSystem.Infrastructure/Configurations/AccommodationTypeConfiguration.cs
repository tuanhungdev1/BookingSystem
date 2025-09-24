using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Infrastructure.Configurations
{
	public class AccommodationTypeConfiguration : IEntityTypeConfiguration<AccommodationType>
	{
		public void Configure(EntityTypeBuilder<AccommodationType> builder)
		{
			// Table name
			builder.ToTable("AccommodationTypes");

			// Primary key (inherited from BaseEntity)
			builder.HasKey(at => at.Id);

			// Properties
			builder.Property(at => at.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(at => at.Description)
				.IsRequired()
				.HasMaxLength(500);

			builder.Property(at => at.Image)
				.HasMaxLength(255);

			builder.Property(at => at.IsActive)
				.IsRequired()
				.HasDefaultValue(true);

			// Navigation properties
			builder.HasMany(at => at.Accommodations)
				.WithOne(a => a.Type)
				.HasForeignKey("AccommodationTypeId")
				.OnDelete(DeleteBehavior.Restrict);

			// Indexes
			builder.HasIndex(at => at.Name)
				.IsUnique();
		}
	}
}
