using BookingSystem.Application.Models.Common;

namespace BookingSystem.Extensions
{
	public static class ConfigureExtensions
	{
		public static IServiceCollection ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
			services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
			services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
			services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
			services.Configure<PaymentGatewaySettings>(configuration.GetSection("PaymentGateway"));
			services.Configure<GoogleAuthSettings>(configuration.GetSection("Authentication:Google"));
			services.Configure<FacebookAuthSettings>(configuration.GetSection("Authentication:Facebook"));
			services.Configure<MomoSettings>(configuration.GetSection("PaymentGateway:Momo"));
			return services;
		}
	}
}
