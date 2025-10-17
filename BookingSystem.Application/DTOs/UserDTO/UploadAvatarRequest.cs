using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.DTOs.UserDTO
{
    public class UploadAvatarRequest
    {
		
			[Required]
			public IFormFile File { get; set; } = default!;
		
	}
}
