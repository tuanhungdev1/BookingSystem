using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Infrastructure.Configurations
{
	public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
	{
		public void Configure(EntityTypeBuilder<Coupon> entity)
		{
			entity.HasKey(c => c.Id);

			entity.Property(c => c.CouponCode)
				.IsRequired()
				.HasMaxLength(50);

			entity.Property(c => c.CouponName)
				.IsRequired()
				.HasMaxLength(200);

			entity.Property(c => c.Description)
				.HasMaxLength(1000);

			entity.Property(c => c.DiscountValue)
				.HasColumnType("decimal(18,2)");

			entity.Property(c => c.MaxDiscountAmount)
				.HasColumnType("decimal(18,2)");

			entity.Property(c => c.MinimumBookingAmount)
				.HasColumnType("decimal(18,2)");

			entity.HasIndex(c => c.CouponCode)
				.IsUnique();

			entity.HasIndex(c => c.IsActive);
			entity.HasIndex(c => c.StartDate);
			entity.HasIndex(c => c.EndDate);

			// Relationships
			entity.HasOne(c => c.CreatedBy)
				.WithMany()
				.HasForeignKey(c => c.CreatedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(c => c.SpecificHomestay)
				.WithMany()
				.HasForeignKey(c => c.SpecificHomestayId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
