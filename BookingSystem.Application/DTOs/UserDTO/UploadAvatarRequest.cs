using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.UserDTO
{
	public class UploadAvatarRequest
	{
		[Required]
		public IFormFile Image { get; set; } = default!;
	}
}
