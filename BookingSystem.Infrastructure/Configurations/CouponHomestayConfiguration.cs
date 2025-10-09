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
	public class CouponHomestayConfiguration : IEntityTypeConfiguration<CouponHomestay>
	{
		public void Configure(EntityTypeBuilder<CouponHomestay> entity)
		{
			entity.HasKey(ch => new { ch.CouponId, ch.HomestayId });

			entity.HasOne(ch => ch.Coupon)
				.WithMany(c => c.CouponHomestays)
				.HasForeignKey(ch => ch.CouponId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(ch => ch.Homestay)
				.WithMany()
				.HasForeignKey(ch => ch.HomestayId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
