using BookingSystem.Application.Attributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.HostProfileDTO
{
	public class UploadBusinessLicenseDto
	{
		[Required(ErrorMessage = "Please upload your business license.")]
		[AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".pdf" })]
		[MaxFileSize(10 * 1024 * 1024)] // 10 MB
		public IFormFile File { get; set; }
	}
}
