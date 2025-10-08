namespace BookingSystem.Domain.Enums
{
	public enum BookingStatus
	{
		Pending = 0,           // Chờ xác nhận
		Confirmed = 1,         // Đã xác nhận
		CheckedIn = 2,         // Đã check-in
		CheckedOut = 3,        // Đã check-out
		Completed = 4,         // Hoàn thành
		Cancelled = 5,         // Đã hủy
		Rejected = 6,          // Bị từ chối
		NoShow = 7            // Không đến
	}
}
