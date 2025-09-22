using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Extensions
{
	public static class ApiBehaviorExtensions
	{
		public static IServiceCollection AddCustomApiBehavior(this IServiceCollection services)
		{
			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					var errors = context.ModelState
						.Where(e => e.Value.Errors.Count > 0)
						.Select(e => new
						{
							Field = e.Key,
							Errors = e.Value.Errors.Select(er => er.ErrorMessage).ToArray()
						}).ToArray();
					var errorResponse = new
					{
						Success = false,
						Message = "Validation errors occurred.",
						Errors = errors
					};
					return new BadRequestObjectResult(errorResponse);
				};
			});
			return services;
		}
	}
}
