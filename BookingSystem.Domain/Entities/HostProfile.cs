using BookingSystem.Domain.Base;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities
{
    public class HostProfile : BaseEntity
    {
		public int UserId { get; set; }

		// Thông tin đăng ký
		public string? BusinessName { get; set; }
		public string? AboutMe { get; set; }
		public string? Languages { get; set; }

		// Thông tin ngân hàng
		public string BankName { get; set; } = string.Empty;
		public string BankAccountNumber { get; set; } = string.Empty;
		public string BankAccountName { get; set; } = string.Empty;

		// Tài liệu xác minh
		public string? IdentityCardFrontUrl { get; set; }
		public string? IdentityCardBackUrl { get; set; }
		public string? BusinessLicenseUrl { get; set; }
		public string? TaxCodeDocumentUrl { get; set; }
		public string? TaxCode { get; set; }

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

		// Trạng thái đơn
		public HostStatus Status { get; set; } = HostStatus.Pending;

		// Thông tin phê duyệt/từ chối
		public int? ReviewedByAdminId { get; set; }
		public DateTime? ReviewedAt { get; set; }
		public string? ReviewNote { get; set; } // Lý do từ chối hoặc ghi chú

		// Thông tin bổ sung
		public string? ApplicantNote { get; set; } // Ghi chú từ người đăng ký
		public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

		// Navigation Properties
		public virtual User User { get; set; } = null!;
		public virtual User? ReviewedByAdmin { get; set; }
	}
}
