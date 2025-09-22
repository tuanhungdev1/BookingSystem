using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
	{
		public void Configure(EntityTypeBuilder<Payment> builder)
		{
			builder.ToTable("Payments");

			builder.Property(p => p.PaymentNumber)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(p => p.Amount)
				.HasPrecision(10, 2);

			builder.Property(p => p.Method)
				.HasConversion<int>();

			builder.Property(p => p.Status)
				.HasConversion<int>()
				.HasDefaultValue(PaymentStatus.Pending);

			builder.Property(p => p.TransactionId)
				.HasMaxLength(100);

			builder.Property(p => p.Notes)
				.HasMaxLength(500);

			builder.HasOne(p => p.Booking)
				.WithMany(b => b.Payments)
				.HasForeignKey(p => p.BookingId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(p => p.PaymentNumber)
				.IsUnique()
				.HasDatabaseName("IX_Payments_PaymentNumber");

			builder.HasIndex(p => p.TransactionId)
				.HasDatabaseName("IX_Payments_TransactionId");
		}
	}
}
