using BookingSystem.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.DI
{
    public static class AddInfrastructure
    {
		public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("ApplicationDB");

			services.AddDbContext<BookingDbContext>(options =>
				options.UseSqlServer(connectionString, b =>
				{
					b.MigrationsAssembly("BookingSystem.Infrastructure");
					//b.EnableRetryOnFailure();
				}));



			return services;
		}
	}
}
