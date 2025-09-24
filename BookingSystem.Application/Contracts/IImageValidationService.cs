using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Contracts
{
	public interface IImageValidationService
	{
		(bool IsValid, string ErrorMessage) ValidateImage(IFormFile file);
		(bool IsValid, string ErrorMessage) ValidateMultipleImages(List<IFormFile> files);
	}
}
