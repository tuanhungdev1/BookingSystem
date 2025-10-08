namespace BookingSystem.Domain.Enums
{
	public enum PaymentStatus
	{
		Pending = 0,           // Chờ thanh toán
		Processing = 1,        // Đang xử lý
		Completed = 2,         // Đã thanh toán
		Failed = 3,            // Thất bại
		Refunded = 4,          // Đã hoàn tiền
		PartiallyRefunded = 5, // Hoàn tiền một phần
		Cancelled = 6          // Đã hủy
	}
}
