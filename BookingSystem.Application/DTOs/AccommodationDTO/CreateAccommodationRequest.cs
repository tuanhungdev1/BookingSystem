using BookingSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.AccommodationDTO
{
	public class CreateAccommodationRequest
	{
		[Required(ErrorMessage = "Name is required")]
		[StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "Description is required")]
		[StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
		public string Description { get; set; } = string.Empty;

		[Required(ErrorMessage = "Address is required")]
		[StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
		public string Address { get; set; } = string.Empty;

		[Required(ErrorMessage = "City is required")]
		[StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
		public string City { get; set; } = string.Empty;

		[Required(ErrorMessage = "Country is required")]
		[StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
		public string Country { get; set; } = string.Empty;

		[StringLength(20, ErrorMessage = "Postal Code cannot exceed 20 characters")]
		public string PostalCode { get; set; } = string.Empty;

		[Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
		public decimal Latitude { get; set; }

		[Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
		public decimal Longitude { get; set; }

		[Phone(ErrorMessage = "Invalid phone number format")]
		public string Phone { get; set; } = string.Empty;

		[EmailAddress(ErrorMessage = "Invalid email format")]
		public string Email { get; set; } = string.Empty;

		[Url(ErrorMessage = "Invalid website URL")]
		public string? Website { get; set; }

		[Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5")]
		public int StarRating { get; set; } = 1;

		public string? MainImage { get; set; }

		public AccommodationType Type { get; set; } = AccommodationType.Hotel;

		public bool IsActive { get; set; } = true;
	}
}
