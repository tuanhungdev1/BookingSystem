namespace BookingSystem.Domain.Exceptions
{
	public abstract class BaseCustomException : Exception
	{
		public int StatusCode { get; }
		public string ErrorCode { get; }
		public object? Details { get; }

		protected BaseCustomException(string message, int statusCode, string errorCode, object? details = null)
			: base(message)
		{
			StatusCode = statusCode;
			ErrorCode = errorCode;
			Details = details;
		}

		protected BaseCustomException(string message, Exception innerException, int statusCode, string errorCode, object? details = null)
			: base(message, innerException)
		{
			StatusCode = statusCode;
			ErrorCode = errorCode;
			Details = details;
		}
	}
}
