using Microsoft.Extensions.DependencyInjection;
using BookingSystem.Application.Contracts;
using BookingSystem.Application.Services;
using BookingSystem.Infrastructure.PaymentGateways;
using BookingSystem.Application.Factories;
using BookingSystem.Application.ServiceUpdate;

namespace BookingSystem.Application.DI
{
	public static class ServiceRegistration
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<IJwtService, JwtService>();
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IEmailService, EmailService>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IImageValidationService, ImageValidationService>();
			services.AddScoped<ICloudinaryService, CloudinaryService>();
			services.AddScoped<IPropertyTypeService, PropertyTypeService>();
			services.AddScoped<IHostProfileService, HostProfileService>();
			services.AddScoped<IRuleService, RuleService>();
			services.AddScoped<IWishlistItemService, WishlistItemService>();
			services.AddScoped<IAmenityService, AmenityService>();
			services.AddScoped<IHomestayService, HomestayService>();
			services.AddScoped<IAvailabilityCalendarService, AvailabilityCalendarService>();
			services.AddScoped<IBookingService, BookingService>();
			services.AddScoped<IReviewService, ReviewService>();
			services.AddScoped<ICouponService , CouponService>();
			services.AddScoped<IUserPreferenceService, UserPreferenceService>();
			services.AddScoped<VNPayService>();
			services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
			services.AddScoped<IPaymentService, PaymentService>();
			services.AddScoped<IDashboardService, DashboardService>();
			services.AddScoped<IGenericExportService, GenericExportService>();
			services.AddScoped<IDashboardExportService, DashboardExportService>();
			services.AddScoped<MomoService>();
			services.AddHttpClient<MomoService>();
			services.AddScoped<CashPaymentService>();
			services.AddHttpContextAccessor();

			return services;
		}
	}
}
