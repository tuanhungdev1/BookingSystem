using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class ImageUpdateDto
	{
		public IFormFile? NewFile { get; set; } // File mới (nếu có)
		public string? Folder { get; set; }
		public Dictionary<string, string>? Tags { get; set; }
	}
}
