using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class ImageResponseDto
	{
		public string PublicId { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
		public string SecureUrl { get; set; } = string.Empty;
	}
}
