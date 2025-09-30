using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
	{
		public void Configure(EntityTypeBuilder<Payment> builder)
		{
			builder.ToTable("Payments");

			// Properties Configuration
			builder.Property(p => p.PaymentAmount)
				.HasPrecision(18, 2);

			builder.Property(p => p.RefundAmount)
				.HasPrecision(18, 2);

			builder.Property(p => p.TransactionId)
				.HasMaxLength(100);

			builder.Property(p => p.PaymentGatewayId)
				.HasMaxLength(100);

			builder.Property(p => p.PaymentGateway)
				.HasMaxLength(50);

			builder.Property(p => p.PaymentNotes)
				.HasMaxLength(1000);

			builder.Property(p => p.FailureReason)
				.HasMaxLength(500);

			// Relationships
			builder.HasOne(p => p.Booking)
				.WithMany(b => b.Payments)
				.HasForeignKey(p => p.BookingId)
				.OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(p => p.TransactionId);
			builder.HasIndex(p => p.PaymentGatewayId);
			builder.HasIndex(p => new { p.BookingId, p.PaymentStatus });
			builder.HasIndex(p => new { p.PaymentMethod, p.PaymentStatus });

			// Query Filters
			builder.HasQueryFilter(p => !p.IsDeleted);
		}
	}
}
