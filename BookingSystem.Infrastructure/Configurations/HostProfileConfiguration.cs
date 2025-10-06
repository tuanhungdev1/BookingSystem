using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Configurations
{
	public class HostProfileConfiguration : IEntityTypeConfiguration<HostProfile>
	{
		public void Configure(EntityTypeBuilder<HostProfile> builder)
		{
			builder.ToTable("HostProfiles");

			// Primary Key
			builder.HasKey(h => h.Id);

			// Relationships
			builder.HasOne(h => h.User)
				   .WithOne(u => u.HostProfile) // giả sử bạn khai báo User có HostProfile
				   .HasForeignKey<HostProfile>(h => h.UserId)
				   .OnDelete(DeleteBehavior.Cascade);

			// Business info
			builder.Property(h => h.BusinessName)
				   .HasMaxLength(200);

			builder.Property(h => h.TaxCode)
				   .HasMaxLength(50);

			// Banking info
			builder.Property(h => h.BankName)
				   .HasMaxLength(100);

			builder.Property(h => h.BankAccountNumber)
				   .HasMaxLength(50);

			builder.Property(h => h.BankAccountName)
				   .HasMaxLength(100);

			// AboutMe & Languages
			builder.Property(h => h.AboutMe)
				   .HasMaxLength(1000);

			builder.Property(h => h.Languages)
				   .HasMaxLength(200);

			// Decimal precision
			builder.Property(h => h.AverageRating)
				   .HasColumnType("decimal(3,2)"); // Ví dụ: 4.75

			// Default values
			builder.Property(h => h.IsActive).HasDefaultValue(true);
			builder.Property(h => h.IsSuperhost).HasDefaultValue(false);
			builder.Property(h => h.TotalHomestays).HasDefaultValue(0);
			builder.Property(h => h.TotalBookings).HasDefaultValue(0);
			builder.Property(h => h.AverageRating).HasDefaultValue(0);
			builder.Property(h => h.ResponseRate).HasDefaultValue(0);
		}
	}
}
