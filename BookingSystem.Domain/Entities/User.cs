using BookingSystem.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Domain.Entities
{
	public class User : IdentityUser<Guid>
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime? DateOfBirth { get; set; }
		public Gender? Gender { get; set; }
		public string? Address { get; set; }
		public string? City { get; set; }
		public string? Country { get; set; }
		public string? PostalCode { get; set; }
		public string? Avatar { get; set; }
		public bool IsActive { get; set; } = true;

		public bool IsEmailConfirmed { get; set; }
		public string? EmailConfirmationToken { get; set; }
		public DateTime? EmailConfirmationTokenExpiry { get; set; }

		// 2FA
		public bool TwoFactorEnabled { get; set; } = false;
		public string? TwoFactorCode { get; set; }
		public DateTime? TwoFactorCodeExpiry { get; set; }
		public bool TwoFactorCodeUsed { get; set; }

		// Refresh Token Management
		public string? RefreshToken { get; set; }
		public DateTime? RefreshTokenExpiryTime { get; set; }

		// Password Reset
		public string? PasswordResetToken { get; set; }
		public DateTime? PasswordResetTokenExpiry { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }

		// Navigation Properties
		public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
		public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
		public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();

		public string FullName => $"{FirstName} {LastName}";

		public void VerifyEmail()
		{
			IsEmailConfirmed = true;
			UpdatedAt = DateTime.UtcNow;
		}

		public void UpdateProfile(
									string firstName,
									string lastName,
									DateTime? dateOfBirth,
									Gender? gender,
									string? address,
									string? city,
									string? country,
									string? postalCode,
									string? avatar)
		{
			FirstName = firstName;
			LastName = lastName;
			DateOfBirth = dateOfBirth;
			Gender = gender;
			Address = address;
			City = city;
			Country = country;
			PostalCode = postalCode;
			Avatar = avatar;

			UpdatedAt = DateTime.UtcNow;
		}

	}
}
