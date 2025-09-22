namespace BookingSystem.Application.Models.Response
{
	public class ErrorResponse
	{
		public string Message { get; set; } = string.Empty;
		public string ErrorCode { get; set; } = string.Empty;
		public int StatusCode { get; set; }
		public string? TraceId { get; set; }
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
		public object? Details { get; set; }
		public string? StackTrace { get; set; }
	}
}
