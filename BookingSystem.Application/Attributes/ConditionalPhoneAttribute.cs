using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Attributes
{
	public class ConditionalPhoneAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
				return ValidationResult.Success; // Bỏ qua nếu null

			var phoneValidator = new PhoneAttribute();
			if (!phoneValidator.IsValid(value))
			{
				return new ValidationResult(ErrorMessage ?? "Invalid phone number format");
			}
			return ValidationResult.Success;
		}
	}
}
