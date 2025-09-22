namespace BookingSystem.Domain.Exceptions
{
	public class ConflictException : BaseCustomException
	{
		public ConflictException(string message)
			: base(message, 409, "CONFLICT")
		{
		}

		public ConflictException(string message, object? details)
			: base(message, 409, "CONFLICT", details)
		{
		}

		public ConflictException(string message, Exception innerException)
			: base(message, innerException, 409, "CONFLICT")
		{
		}
	}
}
