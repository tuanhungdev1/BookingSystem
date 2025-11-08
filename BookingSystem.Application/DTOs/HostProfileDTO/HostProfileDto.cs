using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.HostProfileDTO
{
	public class HostProfileDto
	{
		public int Id { get; set; }
		public int UserId { get; set; }

		// Thông tin đăng ký
		public string? BusinessName { get; set; }
		public string? AboutMe { get; set; }
		public string? Languages { get; set; }

		// Thông tin ngân hàng
		public string BankName { get; set; } = string.Empty;
		public string BankAccountNumber { get; set; } = string.Empty;
		public string BankAccountName { get; set; } = string.Empty;

		// Tài liệu xác minh (URL)
		public string? IdentityCardFrontUrl { get; set; }
		public string? IdentityCardBackUrl { get; set; }
		public string? BusinessLicenseUrl { get; set; }
		public string? TaxCodeDocumentUrl { get; set; }
		public string? TaxCode { get; set; }

		// Statistics
		public int TotalHomestays { get; set; }
		public int TotalBookings { get; set; }
		public decimal AverageRating { get; set; }
		public int ResponseRate { get; set; }
		public TimeSpan? AverageResponseTime { get; set; }

		// Status
		public bool IsActive { get; set; }
		public bool IsSuperhost { get; set; }
		public DateTime RegisteredAsHostAt { get; set; }
		public HostStatus Status { get; set; } 

		// Review
		public int? ReviewedByAdminId { get; set; }
		public DateTime? ReviewedAt { get; set; }
		public string? ReviewNote { get; set; }

		// Thông tin bổ sung
		public string? ApplicantNote { get; set; }
		public DateTime SubmittedAt { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		public bool IsDeleted { get; set; } = false;

		public DateTime? DeletedAt { get; set; }
		public string? CreatedBy { get; set; }
		public string? UpdatedBy { get; set; }
		public string? DeletedBy { get; set; }
	}
}
