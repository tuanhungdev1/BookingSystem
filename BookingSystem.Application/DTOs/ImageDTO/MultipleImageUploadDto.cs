using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.ImageDTO
{
	public class MultipleImageUploadDto
	{
		public List<IFormFile> Files { get; set; } = new();
		public string? Folder { get; set; }
	}
}
