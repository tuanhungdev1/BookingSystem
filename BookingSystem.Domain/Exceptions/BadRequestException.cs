namespace BookingSystem.Domain.Exceptions
{
	public class BadRequestException : BaseCustomException
	{
		public BadRequestException(string message)
			: base(message, 400, "BAD_REQUEST")
		{
		}

		public BadRequestException(string message, object? details)
			: base(message, 400, "BAD_REQUEST", details)
		{
		}

		public BadRequestException(string message, Exception innerException)
			: base(message, innerException, 400, "BAD_REQUEST")
		{
		}
	}
}
