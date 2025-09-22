namespace BookingSystem.Domain.Exceptions
{
	public class NotFoundException : BaseCustomException
	{
		public NotFoundException(string message)
			: base(message, 404, "NOT_FOUND")
		{
		}

		public NotFoundException(string entityName, object key)
			: base($"{entityName} with key '{key}' was not found", 404, "NOT_FOUND")
		{
		}

		public NotFoundException(string message, Exception innerException)
			: base(message, innerException, 404, "NOT_FOUND")
		{
		}
	}
}
