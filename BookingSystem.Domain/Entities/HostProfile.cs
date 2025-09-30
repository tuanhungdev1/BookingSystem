using BookingSystem.Domain.Base;

namespace BookingSystem.Domain.Entities
{
    public class HostProfile : BaseEntity
    {
		public int UserId { get; set; }
		// Thông tin xác minh
		public bool IsVerified { get; set; } = false;
		public DateTime? VerifiedAt { get; set; }
		public string? VerificationDocumentUrl { get; set; }

		// Thông tin kinh doanh
		public string? BusinessName { get; set; }
		public string? BusinessLicense { get; set; }
		public string? TaxCode { get; set; }

		// Thông tin ngân hàng
		public string? BankName { get; set; }
		public string? BankAccountNumber { get; set; }
		public string? BankAccountName { get; set; }

		// Mô tả Host
		public string? AboutMe { get; set; }
		public string? Languages { get; set; }

		// Statistics
		public int TotalHomestays { get; set; } = 0;
		public int TotalBookings { get; set; } = 0;
		public decimal AverageRating { get; set; } = 0;
		public int ResponseRate { get; set; } = 0;
		public TimeSpan? AverageResponseTime { get; set; }

		// Status
		public bool IsActive { get; set; } = true;
		public bool IsSuperhost { get; set; } = false;
		public DateTime RegisteredAsHostAt { get; set; } = DateTime.UtcNow;

		// Navigation property
		public virtual User User { get; set; } = null!;
	}
}
