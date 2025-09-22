using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Application.Mappings
{
	public static class AutoMapperExtensions
	{
		public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
		{
			services.AddAutoMapper(typeof(MappingProfile));
			return services;
		}
	}
}
