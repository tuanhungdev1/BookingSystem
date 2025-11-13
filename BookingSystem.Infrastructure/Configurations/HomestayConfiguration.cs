using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Domain.Entities;
using System.Reflection.Emit;

namespace BookingSystem.Infrastructure.Configurations
{
	public class HomestayConfiguration : IEntityTypeConfiguration<Homestay>
	{
		public void Configure(EntityTypeBuilder<Homestay> builder)
		{
			builder.ToTable("Homestays");

			// Properties Configuration
			builder.Property(h => h.HomestayTitle)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(h => h.HomestayDescription)
				.HasMaxLength(2000);

			builder.Property(h => h.FullAddress)
				.IsRequired()
				.HasMaxLength(500);

			builder.Property(h => h.City)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(h => h.Province)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(h => h.Country)
				.IsRequired()
				.HasMaxLength(100)
				.HasDefaultValue("Vietnam");

			builder.Property(h => h.PostalCode)
				.HasMaxLength(10);

			builder.Property(h => h.Latitude)
				.HasPrecision(10, 7);

			builder.Property(h => h.Longitude)
				.HasPrecision(10, 7);

			builder.Property(h => h.BaseNightlyPrice)
				.HasPrecision(18, 2);

			builder.Property(h => h.WeekendPrice)
				.HasPrecision(18, 2);

			builder.Property(h => h.WeeklyDiscount)
				.HasPrecision(5, 2);

			builder.Property(h => h.MonthlyDiscount)
				.HasPrecision(5, 2);

			builder.Property(h => h.MinimumNights)
				.HasDefaultValue(1);

			builder.Property(h => h.MaximumNights)
				.HasDefaultValue(365);

			builder.Property(h => h.IsInstantBook)
				.HasDefaultValue(false);

			builder.Property(h => h.IsActive)
				.HasDefaultValue(true);

			builder.Property(h => h.IsApproved)
				.HasDefaultValue(false);

			builder.Property(h => h.IsFeatured)
				.HasDefaultValue(false);

			builder.Property(h => h.ApprovedBy)
			.HasMaxLength(100);

			builder.Property(h => h.HomestayTitleNormalized)
		   .HasComputedColumnSql("dbo.RemoveDiacritics([HomestayTitle])", stored: true);

			builder.Property(h => h.HomestayDescriptionNormalized)
				.HasComputedColumnSql("dbo.RemoveDiacritics([HomestayDescription])", stored: true);

			builder.Property(h => h.FullAddressNormalized)
				.HasComputedColumnSql("dbo.RemoveDiacritics([FullAddress])", stored: true);

			builder.Property(h => h.CountryNormalized)
				.HasComputedColumnSql("dbo.RemoveDiacritics([Country])", stored: true);

			builder.Property(h => h.CityNormalized)
				.HasComputedColumnSql("dbo.RemoveDiacritics([City])", stored: true);

			builder.Property(h => h.ProvinceNormalized)
				.HasComputedColumnSql("dbo.RemoveDiacritics([Province])", stored: true);

			builder.Property(h => h.SearchKeywordsNormalized)
				.HasComputedColumnSql("dbo.RemoveDiacritics([SearchKeywords])", stored: true);

			builder.Property(h => h.SlugNormalized)
				.HasComputedColumnSql("dbo.RemoveDiacritics([Slug])", stored: true);

			// Relationships
			builder.HasOne(h => h.Owner)
				.WithMany(u => u.OwnedHomestays)
				.HasForeignKey(h => h.OwnerId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(h => h.PropertyType)
				.WithMany(pt => pt.Homestays)
				.HasForeignKey(h => h.PropertyTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			// Indexes
			builder.HasIndex(h => new { h.City, h.Province, h.Country });
			builder.HasIndex(h => new { h.Latitude, h.Longitude });
			builder.HasIndex(h => new { h.IsActive, h.IsApproved, h.IsDeleted });
			builder.HasIndex(h => h.BaseNightlyPrice);
			builder.HasIndex(h => h.MaximumGuests);
			builder.HasIndex(h => h.IsFeatured);

			// Query Filters
			builder.HasQueryFilter(h => !h.IsDeleted);
		}
	}
}
