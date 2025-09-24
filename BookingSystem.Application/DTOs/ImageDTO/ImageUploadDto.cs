using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class ImageUploadDto
	{
		public IFormFile File { get; set; } = null!;
		public string? Folder { get; set; }
	}
}
