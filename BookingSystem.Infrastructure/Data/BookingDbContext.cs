using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BookingSystem.Domain.Enums;

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

			// Seed data
			//SeedData(builder);
		}

		private void SeedData(ModelBuilder builder)
		{
			SeedRoles(builder);
			SeedUsers(builder);
			SeedUserRoles(builder);
			SeedAmenities(builder);
			SeedAccommodations(builder);
		}
		private void SeedRoles(ModelBuilder builder)
		{
			var roles = new[]
			{
			new Role
			{
				Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
				Name = "SuperAdmin",
				NormalizedName = "SUPERADMIN",
				Description = "Super Administrator with full system access",
				CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			},
			new Role
			{
				Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
				Name = "Admin",
				NormalizedName = "ADMIN",
				Description = "Administrator with management access",
				CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			},
			new Role
			{
				Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
				Name = "AccommodationManager",
				NormalizedName = "ACCOMMODATIONMANAGER",
				Description = "Accommodation Manager with Accommodation-specific access",
				CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			},
			new Role
			{
				Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
				Name = "Staff",
				NormalizedName = "STAFF",
				Description = "Hotel Staff with limited access",
				CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			},
			new Role
			{
				Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
				Name = "Customer",
				NormalizedName = "CUSTOMER",
				Description = "Regular customer account",
				CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			}
		};

			builder.Entity<Role>().HasData(roles);
		}

		private void SeedUsers(ModelBuilder builder)
		{
			var hasher = new PasswordHasher<User>();
			var users = new List<User>();

			// --- Admin ---
			var admin = new User
			{
				Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
				UserName = "admin@booking.com",
				NormalizedUserName = "ADMIN@BOOKING.COM",
				Email = "admin@booking.com",
				NormalizedEmail = "ADMIN@BOOKING.COM",
				EmailConfirmed = true,
				FirstName = "System",
				LastName = "Administrator",
				DateOfBirth = new DateTime(1990, 1, 1),
				Gender = Gender.Male,
				Address = "1 Nguyen Hue Street",
				City = "Ho Chi Minh City",
				Country = "Vietnam",
				PostalCode = "700000",
				Avatar = "/images/users/admin.png",
				IsActive = true,
				CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				LastLoginAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			};
			admin.PasswordHash = hasher.HashPassword(admin, "Admin@123456");
			users.Add(admin);

			// --- Manager ---
			var manager = new User
			{
				Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
				UserName = "manager@hotel.com",
				NormalizedUserName = "MANAGER@HOTEL.COM",
				Email = "manager@hotel.com",
				NormalizedEmail = "MANAGER@HOTEL.COM",
				EmailConfirmed = true,
				FirstName = "Anna",
				LastName = "Nguyen",
				DateOfBirth = new DateTime(1985, 5, 15),
				Gender = Gender.Female,
				Address = "45 Tran Hung Dao",
				City = "Da Nang",
				Country = "Vietnam",
				PostalCode = "550000",
				Avatar = "/images/users/manager.png",
				IsActive = true,
				CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				LastLoginAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			};
			manager.PasswordHash = hasher.HashPassword(manager, "Manager@123");
			users.Add(manager);

			// --- Customer ---
			var customer = new User
			{
				Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
				UserName = "customer@example.com",
				NormalizedUserName = "CUSTOMER@EXAMPLE.COM",
				Email = "customer@example.com",
				NormalizedEmail = "CUSTOMER@EXAMPLE.COM",
				EmailConfirmed = true,
				FirstName = "John",
				LastName = "Doe",
				DateOfBirth = new DateTime(1995, 10, 20),
				PhoneNumber = "+84901234567",
				Gender = Gender.Male,
				Address = "123 Le Loi Street",
				City = "Ho Chi Minh City",
				Country = "Vietnam",
				PostalCode = "700000",
				Avatar = "/images/users/customer.png",
				IsActive = true,
				CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				LastLoginAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			};
			customer.PasswordHash = hasher.HashPassword(customer, "Customer@123");
			users.Add(customer);
			// Seed tất cả vào DB
			builder.Entity<User>().HasData(users);
		}

		private void SeedUserRoles(ModelBuilder builder)
		{
			var userRoles = new[]
			{
			// SuperAdmin
			new UserRole
			{
				UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
				RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
				AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				AssignedBy = "System"
			},
			// Admin
			new UserRole
			{
				UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
				RoleId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
				AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				AssignedBy = "System"
			},
			// AccommodationManager
			new UserRole
			{
				UserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
				RoleId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
				AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				AssignedBy = "System"
			},
			// Staff
			new UserRole
			{
				UserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
				RoleId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
				AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				AssignedBy = "System"
			},
			// Staff
			new UserRole
			{
				UserId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
				RoleId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
				AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				AssignedBy = "System"
			},
			// Customer
			new UserRole
			{
				UserId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
				RoleId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
				AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				AssignedBy = "System"
			},
		};

			builder.Entity<UserRole>().HasData(userRoles);
		}

		private void SeedAmenities(ModelBuilder builder)
		{
			var amenities = new[]
			{
				// Hotel Amenities
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000001"), Name = "Free Wi-Fi",        Category = "Hotel", Icon = "wifi",       IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000002"), Name = "Swimming Pool",    Category = "Hotel", Icon = "pool",       IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000003"), Name = "Fitness Center",   Category = "Hotel", Icon = "gym",        IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000004"), Name = "Restaurant",       Category = "Hotel", Icon = "restaurant", IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000005"), Name = "Bar",              Category = "Hotel", Icon = "bar",        IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000006"), Name = "Spa",              Category = "Hotel", Icon = "spa",        IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000007"), Name = "Business Center",  Category = "Hotel", Icon = "business",   IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000008"), Name = "Conference Room",  Category = "Hotel", Icon = "meeting",    IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000009"), Name = "Parking",          Category = "Hotel", Icon = "parking",    IsActive = true },
				new Amenity { Id = Guid.Parse("aaaa1111-0000-0000-0000-000000000010"), Name = "24-Hour Front Desk", Category = "Hotel", Icon = "reception", IsActive = true },

				// Room Amenities
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000001"), Name = "Air Conditioning", Category = "Room", Icon = "ac",       IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000002"), Name = "Mini Bar",        Category = "Room", Icon = "minibar",  IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000003"), Name = "Safe",            Category = "Room", Icon = "safe",     IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000004"), Name = "TV",              Category = "Room", Icon = "tv",       IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000005"), Name = "Balcony",         Category = "Room", Icon = "balcony",  IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000006"), Name = "Sea View",        Category = "Room", Icon = "sea",      IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000007"), Name = "City View",       Category = "Room", Icon = "city",     IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000008"), Name = "Room Service",    Category = "Room", Icon = "service",  IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000009"), Name = "Coffee Machine",  Category = "Room", Icon = "coffee",   IsActive = true },
				new Amenity { Id = Guid.Parse("bbbb2222-0000-0000-0000-000000000010"), Name = "Bathtub",         Category = "Room", Icon = "bathtub",  IsActive = true }
			};

			builder.Entity<Amenity>().HasData(amenities);
		}

		private void SeedAccommodations(ModelBuilder builder)
		{
			var accommodations = new[]
			{
				new Accommodation
				{
					Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
					Name = "Grand Hotel Saigon",
					Description = "Luxury 5-star hotel in the heart of Ho Chi Minh City, offering world-class amenities and exceptional service.",
					Address = "8 Dong Khoi Street, District 1",
					City = "Ho Chi Minh City",
					Country = "Vietnam",
					PostalCode = "70000",
					Latitude = 10.7769m,
					Longitude = 106.7009m,
					Phone = "+84-28-3823-0163",
					Email = "info@grandhotelsaigon.com",
					Website = "https://www.grandhotelsaigon.com",
					StarRating = 5,
					MainImage = "/images/accommodations/grand-hotel-saigon.jpg",
					Type = AccommodationType.Hotel,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("23456789-2345-2345-2345-234567890123"),
					Name = "Hanoi Elegance Hotel",
					Description = "Boutique hotel located in Hanoi's Old Quarter, combining traditional Vietnamese charm with modern amenities.",
					Address = "25 Hang Be Street, Hoan Kiem District",
					City = "Hanoi",
					Country = "Vietnam",
					PostalCode = "10000",
					Latitude = 21.0285m,
					Longitude = 105.8542m,
					Phone = "+84-24-3936-1688",
					Email = "reservations@hanoielegance.com",
					Website = "https://www.hanoielegance.com",
					StarRating = 4,
					MainImage = "/images/accommodations/hanoi-elegance.jpg",
					Type = AccommodationType.Hotel,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("34567890-3456-3456-3456-345678901234"),
					Name = "Vinpearl Resort Nha Trang",
					Description = "Luxury beach resort with private beach, infinity pool, and family-friendly services.",
					Address = "Hon Tre Island",
					City = "Nha Trang",
					Country = "Vietnam",
					PostalCode = "650000",
					Latitude = 12.2388m,
					Longitude = 109.1967m,
					Phone = "+84-258-359-8900",
					Email = "contact@vinpearl.com",
					Website = "https://vinpearl.com",
					StarRating = 5,
					MainImage = "/images/accommodations/vinpearl-nhatrang.jpg",
					Type = AccommodationType.Resort,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("45678901-4567-4567-4567-456789012345"),
					Name = "Hoi An Riverside Homestay",
					Description = "Cozy homestay by the river with authentic Vietnamese family hospitality.",
					Address = "Cam Thanh, Hoi An",
					City = "Hoi An",
					Country = "Vietnam",
					PostalCode = "560000",
					Latitude = 15.8801m,
					Longitude = 108.3380m,
					Phone = "+84-235-392-1234",
					Email = "info@hoianhomestay.com",
					Website = null,
					StarRating = 3,
					MainImage = "/images/accommodations/hoian-homestay.jpg",
					Type = AccommodationType.Homestay,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("56789012-5678-5678-5678-567890123456"),
					Name = "Da Lat Mountain Villa",
					Description = "Private luxury villa with mountain views, garden, and fireplace, ideal for families and groups.",
					Address = "Ward 10, Da Lat",
					City = "Da Lat",
					Country = "Vietnam",
					PostalCode = "670000",
					Latitude = 11.9404m,
					Longitude = 108.4583m,
					Phone = "+84-263-355-8888",
					Email = "booking@dalatvilla.com",
					Website = null,
					StarRating = 4,
					MainImage = "/images/accommodations/dalat-villa.jpg",
					Type = AccommodationType.Villa,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("67890123-6789-6789-6789-678901234567"),
					Name = "Saigon Sky Apartment",
					Description = "Modern serviced apartment with full kitchen, rooftop pool, and city skyline view.",
					Address = "72 Nguyen Hue Boulevard, District 1",
					City = "Ho Chi Minh City",
					Country = "Vietnam",
					PostalCode = "70000",
					Latitude = 10.7758m,
					Longitude = 106.7019m,
					Phone = "+84-28-3911-2233",
					Email = "stay@saigonsky.com",
					Website = null,
					StarRating = 4,
					MainImage = "/images/accommodations/saigon-apartment.jpg",
					Type = AccommodationType.Apartment,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("78901234-7890-7890-7890-789012345678"),
					Name = "Phu Quoc Paradise Resort",
					Description = "Beachfront resort with spa, seafood restaurant, and water activities.",
					Address = "Ong Lang Beach",
					City = "Phu Quoc",
					Country = "Vietnam",
					PostalCode = "920000",
					Latitude = 10.2270m,
					Longitude = 103.9637m,
					Phone = "+84-297-384-5678",
					Email = "info@phuquocparadise.com",
					Website = null,
					StarRating = 5,
					MainImage = "/images/accommodations/phuquoc-resort.jpg",
					Type = AccommodationType.Resort,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("89012345-8901-8901-8901-890123456789"),
					Name = "Sapa Eco Homestay",
					Description = "Eco-friendly homestay with mountain trekking tours and ethnic minority culture experience.",
					Address = "Lao Chai Village",
					City = "Sapa",
					Country = "Vietnam",
					PostalCode = "330000",
					Latitude = 22.3350m,
					Longitude = 103.8438m,
					Phone = "+84-214-388-2222",
					Email = "contact@sapaeco.com",
					Website = null,
					StarRating = 3,
					MainImage = "/images/accommodations/sapa-homestay.jpg",
					Type = AccommodationType.Homestay,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("90123456-9012-9012-9012-901234567890"),
					Name = "InterContinental Danang Sun Peninsula Resort",
					Description = "Ultra-luxury resort designed by Bill Bensley, offering private villas with sea views.",
					Address = "Bai Bac, Son Tra Peninsula",
					City = "Da Nang",
					Country = "Vietnam",
					PostalCode = "550000",
					Latitude = 16.1241m,
					Longitude = 108.3278m,
					Phone = "+84-236-393-8888",
					Email = "danang@ihg.com",
					Website = "https://www.danang.intercontinental.com",
					StarRating = 5,
					MainImage = "/images/accommodations/intercontinental-danang.jpg",
					Type = AccommodationType.Resort,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				},
				new Accommodation
				{
					Id = Guid.Parse("01234567-0123-0123-0123-012345678901"),
					Name = "Ha Long Bay View Apartment",
					Description = "Serviced apartment with balcony overlooking Ha Long Bay, ideal for long stays.",
					Address = "Bai Chay Ward",
					City = "Ha Long",
					Country = "Vietnam",
					PostalCode = "200000",
					Latitude = 20.9590m,
					Longitude = 107.0448m,
					Phone = "+84-203-352-7777",
					Email = "stay@halongapartment.com",
					Website = null,
					StarRating = 4,
					MainImage = "/images/accommodations/halong-apartment.jpg",
					Type = AccommodationType.Apartment,
					IsActive = true,
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					CreatedBy = "System"
				}
			};

			builder.Entity<Accommodation>().HasData(accommodations);
		}

	}
}
