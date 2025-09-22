namespace BookingSystem.Domain.Exceptions
{
	public class ForbiddenException : BaseCustomException
	{
		public ForbiddenException(string message = "Forbidden access")
			: base(message, 403, "FORBIDDEN")
		{
		}

		public ForbiddenException(string message, Exception innerException)
			: base(message, innerException, 403, "FORBIDDEN")
		{
		}
	}
}
