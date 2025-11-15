using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.DTOs.CouponDTO
{
	public class ValidateCouponDto
	{
		[Required(ErrorMessage = "Coupon code is required")]
		public string CouponCode { get; set; } = string.Empty;

		[Required(ErrorMessage = "Homestay ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Invalid homestay ID")]
		public int HomestayId { get; set; }

		[Required(ErrorMessage = "Booking amount is required")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Booking amount must be greater than 0")]
		public decimal BookingAmount { get; set; }

		[Required(ErrorMessage = "Number of nights is required")]
		[Range(1, 365, ErrorMessage = "Number of nights must be between 1 and 365")]
		public int NumberOfNights { get; set; }

		[Required(ErrorMessage = "Check-in date is required")]
		public DateTime CheckInDate { get; set; }

		[Required(ErrorMessage = "Check-out date is required")]
		public DateTime CheckOutDate { get; set; }

		public int BookingId { get; set; }
	}
}
