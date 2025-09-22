using BookingSystem.Middleware;

namespace BookingSystem.Extensions
{
	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseApplicationMiddlewares(this IApplicationBuilder app)
		{
			app.UseMiddleware<LoggingMiddleware>();
			app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
			return app;
		}
	}
}
