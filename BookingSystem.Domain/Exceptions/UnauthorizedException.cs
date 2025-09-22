namespace BookingSystem.Domain.Exceptions
{
	public class UnauthorizedException : BaseCustomException
	{
		public UnauthorizedException(string message = "Unauthorized access")
			: base(message, 401, "UNAUTHORIZED")
		{
		}

		public UnauthorizedException(string message, Exception innerException)
			: base(message, innerException, 401, "UNAUTHORIZED")
		{
		}
	}
}
