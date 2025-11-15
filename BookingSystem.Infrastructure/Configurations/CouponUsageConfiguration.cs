using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Infrastructure.Configurations
{
	public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
	{
		public void Configure(EntityTypeBuilder<CouponUsage> entity)
		{
			entity.HasKey(cu => cu.Id);

			entity.Property(cu => cu.DiscountAmount)
				.HasColumnType("decimal(18,2)");

			entity.HasIndex(cu => cu.UserId);
			entity.HasIndex(cu => cu.CouponId);
			entity.HasIndex(cu => cu.UsedAt);

			// Relationships
			entity.HasOne(cu => cu.Coupon)
				.WithMany(c => c.CouponUsages)
				.HasForeignKey(cu => cu.CouponId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(cu => cu.User)
				.WithMany()
				.HasForeignKey(cu => cu.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(cu => cu.Booking)
				.WithMany()
				.HasForeignKey(cu => cu.BookingId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
