using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace BookingSystem.Infrastructure.Data
{
	public class BookingDbContext : IdentityDbContext<User, Role, Guid,
	IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
	IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
	{
		public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
		{
		}

		public DbSet<Accommodation> Accommodations { get; set; }
		public DbSet<AccommodationType> AccommodationTypes { get; set; }
		public DbSet<RoomType> RoomTypes { get; set; }
		public DbSet<Room> Rooms { get; set; }
		public DbSet<Amenity> Amenities { get; set; }
		public DbSet<AccommodationAmenity> AccommodationAmenities { get; set; }
		public DbSet<RoomTypeAmenity> RoomTypeAmenities { get; set; }
		public DbSet<Booking> Bookings { get; set; }
		public DbSet<BookingRoom> BookingRooms { get; set; }
		public DbSet<BookingGuest> BookingGuests { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Review> Reviews { get; set; }
		public DbSet<AccommodationImage> AccommodationImages { get; set; }
		public DbSet<RoomTypeImage> RoomTypeImages { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// Apply all configurations
			builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

			// Configure Identity tables
			builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
			builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
			builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
			builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
		}
	}
}
