namespace BookingSystem.Application.Models.Responses
{
	public class ApiResponse<T> : BaseResponse
	{
		public T? Data { get; set; }

		public ApiResponse()
		{
		}

		public ApiResponse(T data, string message = "Success")
		{
			Data = data;
			Message = message;
			Success = true;
		}

		public ApiResponse(string message, bool success = false)
		{
			Message = message;
			Success = success;
		}
	}

	public class ApiResponse : BaseResponse
	{
		public ApiResponse()
		{
		}

		public ApiResponse(string message, bool success = true)
		{
			Message = message;
			Success = success;
		}

		public static ApiResponse<T> Ok<T>(T data, string message = "Success")
		{
			return new ApiResponse<T>(data, message);
		}

		public static ApiResponse<T> Fail<T>(string message)
		{
			return new ApiResponse<T>(message, false);
		}

		public static ApiResponse Ok(string message = "Success")
		{
			return new ApiResponse(message, true);
		}

		public static ApiResponse Fail(string message)
		{
			return new ApiResponse(message, false);
		}
	}
}
