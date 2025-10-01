using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.HostProfileDTO
{
	public class CreateHostProfileDto
	{
		public int UserId { get; set; }

		// Registration Info
		[Required(ErrorMessage = "Business name is required.")]
		[MaxLength(200, ErrorMessage = "Business name cannot exceed 200 characters.")]
		public string BusinessName { get; set; } = string.Empty;

		[MaxLength(1000, ErrorMessage = "About me cannot exceed 1000 characters.")]
		public string? AboutMe { get; set; }

		public string? Languages { get; set; }

		// Bank Information
		[Required(ErrorMessage = "Bank name is required.")]
		public string BankName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Bank account number is required.")]
		[RegularExpression(@"^\d+$", ErrorMessage = "Bank account number must contain only digits.")]
		public string BankAccountNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "Bank account holder name is required.")]
		public string BankAccountName { get; set; } = string.Empty;

		// Verification Files
		[Required(ErrorMessage = "Please upload the front side of your ID card.")]
		public IFormFile IdentityCardFrontFile { get; set; } = null!;

		[Required(ErrorMessage = "Please upload the back side of your ID card.")]
		public IFormFile IdentityCardBackFile { get; set; } = null!;

		public IFormFile? BusinessLicenseFile { get; set; }
		public IFormFile? TaxCodeDocumentFile { get; set; }

		// Additional Note from Applicant
		public string? ApplicantNote { get; set; }
	}
}
