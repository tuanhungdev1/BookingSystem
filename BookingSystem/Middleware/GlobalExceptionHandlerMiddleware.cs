using BookingSystem.Application.Models.Response;
using BookingSystem.Domain.Exceptions;
using System.Text.Json;

namespace BookingSystem.Middleware
{
	public class GlobalExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
		private readonly IWebHostEnvironment _environment;

		public GlobalExceptionHandlerMiddleware(
			RequestDelegate next,
			ILogger<GlobalExceptionHandlerMiddleware> logger,
			IWebHostEnvironment environment)
		{
			_next = next;
			_logger = logger;
			_environment = environment;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unhandled exception occurred while processing request {RequestPath}",
					context.Request.Path);

				await HandleExceptionAsync(context, ex);
			}
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			var response = context.Response;
			response.ContentType = "application/json";

			var errorResponse = exception switch
			{
				BaseCustomException customEx => CreateCustomErrorResponse(customEx, context),
				UnauthorizedAccessException unauthorized => CreateErrorResponse(unauthorized.Message, 401, "UNAUTHORIZED", context),
				ArgumentException argumentEx => CreateErrorResponse(argumentEx.Message, 400, "INVALID_ARGUMENT", context),
				InvalidOperationException invalidOpEx => CreateErrorResponse(invalidOpEx.Message, 400, "INVALID_OPERATION", context),
				TimeoutException timeoutEx => CreateErrorResponse(timeoutEx.Message, 408, "TIMEOUT", context),
				NotImplementedException notImplEx => CreateErrorResponse(notImplEx.Message, 501, "NOT_IMPLEMENTED", context),
				TaskCanceledException taskCanceledEx => CreateErrorResponse(taskCanceledEx.Message, 499, "REQUEST_CANCELLED", context),
				_ => CreateErrorResponse(exception.Message, 500, "INTERNAL_SERVER_ERROR", context)
			};

			response.StatusCode = errorResponse.StatusCode;

			var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = _environment.IsDevelopment()
			});

			await response.WriteAsync(jsonResponse);
		}

		private ErrorResponse CreateCustomErrorResponse(BaseCustomException exception, HttpContext context)
		{
			var errorResponse = new ErrorResponse
			{
				Message = exception.Message,
				ErrorCode = exception.ErrorCode,
				StatusCode = exception.StatusCode,
				TraceId = context.TraceIdentifier,
				Details = exception.Details
			};

			if (_environment.IsDevelopment())
			{
				errorResponse.StackTrace = exception.StackTrace;
			}

			return errorResponse;
		}

		private ErrorResponse CreateErrorResponse(string message, int statusCode, string errorCode, HttpContext context)
		{
			return new ErrorResponse
			{
				Message = message,
				ErrorCode = errorCode,
				StatusCode = statusCode,
				TraceId = context.TraceIdentifier
			};
		}
	}
}
