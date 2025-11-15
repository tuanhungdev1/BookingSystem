using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Attributes
{
	public class ConditionalEmailAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
				return ValidationResult.Success; // Bỏ qua nếu null

			var emailValidator = new EmailAddressAttribute();
			if (!emailValidator.IsValid(value))
			{
				return new ValidationResult(ErrorMessage ?? "Invalid email format");
			}
			return ValidationResult.Success;
		}
	}
}
