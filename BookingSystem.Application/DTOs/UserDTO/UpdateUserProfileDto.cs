using System;
using System.ComponentModel.DataAnnotations;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs.Users
{
	public class UpdateUserProfileDto
	{
		[Required(ErrorMessage = "Họ không được để trống.")]
		[StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự.")]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Tên không được để trống.")]
		[StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự.")]
		public string LastName { get; set; } = string.Empty;

		[DataType(DataType.Date)]
		public DateTime? DateOfBirth { get; set; }

		public Gender? Gender { get; set; }

		[StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
		public string? Address { get; set; }

		[StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
		public string? City { get; set; }

		[StringLength(100, ErrorMessage = "Tên quốc gia không được vượt quá 100 ký tự.")]
		public string? Country { get; set; }

		[StringLength(20, ErrorMessage = "Mã bưu điện không được vượt quá 20 ký tự.")]
		public string? PostalCode { get; set; }

		[Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
		[StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
		public string? PhoneNumber { get; set; }
	}
}
