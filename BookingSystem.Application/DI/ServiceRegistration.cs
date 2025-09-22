using Microsoft.Extensions.DependencyInjection;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.Services;

namespace BookingSystem.Application.DI
{
	public static class ServiceRegistration
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<IJwtService, JwtService>();
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IEmailService, EmailService>();
			services.AddScoped<IAccommodationService, AccommodationService>();
			services.AddScoped<IUserService, UserService>();
			return services;
		}
	}
}
