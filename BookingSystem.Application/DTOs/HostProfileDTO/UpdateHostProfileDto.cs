using Microsoft.AspNetCore.Http;

namespace BookingSystem.Application.DTOs.HostProfileDTO
{
	public class UpdateHostProfileDto
	{
		public string? BusinessName { get; set; }
		public string? AboutMe { get; set; }
		public string? Languages { get; set; }

		public string? BankName { get; set; }
		public string? BankAccountNumber { get; set; }
		public string? BankAccountName { get; set; }

		// Cho phép update file mới
		public IFormFile? IdentityCardFrontFile { get; set; }
		public IFormFile? IdentityCardBackFile { get; set; }
		public IFormFile? BusinessLicenseFile { get; set; }
		public IFormFile? TaxCodeDocumentFile { get; set; }

		public string? TaxCode { get; set; }
		public string? ApplicantNote { get; set; }
	}
}
