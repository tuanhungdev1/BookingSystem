namespace BookingSystem.Application.Models.Responses
{
	public class BaseResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
		public string? TraceId { get; set; }
	}
}
