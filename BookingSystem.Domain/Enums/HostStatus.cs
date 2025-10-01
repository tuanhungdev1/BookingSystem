namespace BookingSystem.Domain.Enums
{
	public enum HostStatus
	{
		Pending = 0,           // Chờ xét duyệt
		UnderReview = 1,       // Đang xem xét
		Approved = 2,          // Đã chấp nhận
		Rejected = 3,          // Đã từ chối
		RequiresMoreInfo = 4,  // Yêu cầu bổ sung thông tin
		Cancelled = 5          // Người dùng hủy đơn
	}
}
