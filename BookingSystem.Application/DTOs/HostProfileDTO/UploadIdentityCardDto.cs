using BookingSystem.Application.Attributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.HostProfileDTO
{
	public class UploadIdentityCardDto
	{
		[Required(ErrorMessage = "Please upload the front side of your ID card.")]
		[AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
		[MaxFileSize(5 * 1024 * 1024)] // 5 MB
		public IFormFile FrontImage { get; set; }

		[Required(ErrorMessage = "Please upload the back side of your ID card.")]
		[AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
		[MaxFileSize(5 * 1024 * 1024)] // 5 MB
		public IFormFile BackImage { get; set; }
	}

}
