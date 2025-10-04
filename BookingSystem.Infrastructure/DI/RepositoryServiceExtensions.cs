using BookingSystem.Domain.Base;
using BookingSystem.Domain.Repositories;
using BookingSystem.Infrastructure.Persistence;
using BookingSystem.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Infrastructure.DI
{
    public static class RepositoryServiceExtensions
    {
		public static IServiceCollection AddRepositories(this IServiceCollection services)
		{
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<IHomestayRepository, HomestayRepository>();
			services.AddScoped<IPropertyTypeRepository, PropertyTypeRepository>();
			services.AddScoped<IAmenityRepository, AmenityRepository>();
			services.AddScoped<IHostProfileRepository, HostProfileRepository>();
			return services;
		}
	}
}
