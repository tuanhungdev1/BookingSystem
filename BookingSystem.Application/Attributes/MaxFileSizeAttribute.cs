using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Attributes
{
	// Validate max file size
	public class MaxFileSizeAttribute : ValidationAttribute
	{
		private readonly int _maxFileSize;
		public MaxFileSizeAttribute(int maxFileSize)
		{
			_maxFileSize = maxFileSize;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var file = value as IFormFile;
			if (file != null && file.Length > _maxFileSize)
			{
				return new ValidationResult($"The file size exceeds the maximum allowed limit of {_maxFileSize / 1024 / 1024} MB.");
			}
			return ValidationResult.Success;
		}
	}
}
