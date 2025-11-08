using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace BookingSystem.Infrastructure.Data
{
	public class BookingDbContext : IdentityDbContext<User, IdentityRole<int>, int>
	{
		public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
		{
		}

		// DbSets
		public DbSet<Homestay> Homestays { get; set; }
		public DbSet<PropertyType> PropertyTypes { get; set; }
		public DbSet<Amenity> Amenities { get; set; }
		public DbSet<Rule> Rules { get; set; }
		public DbSet<HomestayAmenity> HomestayAmenities { get; set; }
		public DbSet<HomestayRule> HomestayRules { get; set; }
		public DbSet<HomestayImage> HomestayImages { get; set; }
		public DbSet<Booking> Bookings { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Review> Reviews { get; set; }
		public DbSet<WishlistItem> WishlistItems { get; set; }
		public DbSet<AvailabilityCalendar> AvailabilityCalendars { get; set; }
		public DbSet<UserPreference> UserPreferences { get; set; }
		public DbSet<Coupon> Coupons { get; set; }
		public DbSet<CouponUsage> CouponUsages { get; set; }
		public DbSet<CouponHomestay> CouponHomestays { get; set; }
		public DbSet<HostProfile> HostProfiles { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// Apply all configurations
			builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

			// Configure Identity tables
			builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
			builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
			builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
			builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
		}
	}
}
