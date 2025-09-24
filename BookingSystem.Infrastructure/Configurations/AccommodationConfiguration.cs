using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Infrastructure.Configurations
{
	public class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
	{
		public void Configure(EntityTypeBuilder<Accommodation> builder)
		{
			builder.ToTable("Accommodations");

			builder.Property(h => h.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(h => h.Description)
				.IsRequired()
				.HasMaxLength(2000);

			builder.Property(h => h.Address)
				.IsRequired()
				.HasMaxLength(300);

			builder.Property(h => h.City)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(h => h.Country)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(h => h.PostalCode)
				.HasMaxLength(20);

			builder.Property(h => h.Latitude)
				.HasPrecision(10, 8);

			builder.Property(h => h.Longitude)
				.HasPrecision(11, 8);

			builder.Property(h => h.Phone)
				.IsRequired()
				.HasMaxLength(20);

			builder.Property(h => h.Email)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(h => h.Website)
				.HasMaxLength(200);

			builder.Property(h => h.StarRating)
				.HasDefaultValue(1);

			builder.Property(h => h.MainImage)
				.HasMaxLength(500);

			builder.Property(h => h.Type)
				.IsRequired()
				.HasDefaultValue(AccommodationType.Hotel);

			builder.HasIndex(h => h.Name)
				.HasDatabaseName("IX_Hotels_Name");

			builder.HasIndex(h => new { h.City, h.Country })
				.HasDatabaseName("IX_Hotels_Location");

			builder.HasIndex(h => h.IsActive)
				.HasDatabaseName("IX_Hotels_IsActive");
		}
	}
}
