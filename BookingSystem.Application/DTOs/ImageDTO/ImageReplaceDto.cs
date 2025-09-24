using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class ImageReplaceDto
	{
		public string OldPublicId { get; set; } = string.Empty;
		public IFormFile NewFile { get; set; } = null!;
		public string? Folder { get; set; }
	}
}
