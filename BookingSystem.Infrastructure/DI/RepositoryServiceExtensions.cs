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
			services.AddScoped<IAccommodationRepository, AccommodationRepository>();
			services.AddScoped<IAccommodationTypeRepository, AccommodationTypeRepository>();
			// Thêm các repository khác tại đây

			return services;
		}
	}
}
