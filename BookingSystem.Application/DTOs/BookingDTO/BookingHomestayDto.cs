namespace BookingSystem.Application.DTOs.BookingDTO
{
	public class BookingHomestayDto
	{
		public int Id { get; set; }
		public string HomestayTitle { get; set; } = string.Empty;
		public string FullAddress { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Province { get; set; } = string.Empty;
		public TimeOnly CheckInTime { get; set; }
		public TimeOnly CheckOutTime { get; set; }
		public string? MainImageUrl { get; set; }
		public string PropertyTypeName { get; set; } = string.Empty;

		// Host Information
		public int OwnerId { get; set; }
		public string OwnerName { get; set; } = string.Empty;
		public string? OwnerPhone { get; set; }
		public string? OwnerEmail { get; set; }
		public string? OwnerAvatar { get; set; }
	}
}
