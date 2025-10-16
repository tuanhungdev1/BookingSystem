using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Extensions
{
	public static class ServiceExtensions
	{
		public static void ConfigureIISIntegration(this IServiceCollection services)
		{
			services.Configure<IISOptions>(options =>
			{
				options.AutomaticAuthentication = true;
				options.ForwardClientCertificate = true;
				options.AuthenticationDisplayName = "Windows";
			});
		}

		public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
		{
			// Identity Configuration
			services.AddIdentity<User, IdentityRole<int>>(options =>
			{
				// Password settings
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 6;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = false;

				// User settings
				options.User.RequireUniqueEmail = true;
				options.SignIn.RequireConfirmedEmail = false; // Custom email confirmation

				// Lockout settings
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;
			})
			.AddEntityFrameworkStores<BookingDbContext>()
			.AddDefaultTokenProviders();

			// Cấu hình Cookie để không redirect cho API
			services.ConfigureApplicationCookie(options =>
			{
				// Disable redirects for API
				options.Events.OnRedirectToLogin = context =>
				{
					if (context.Request.Path.StartsWithSegments("/api"))
					{
						context.Response.StatusCode = 401;
						return Task.CompletedTask;
					}
					context.Response.Redirect(context.RedirectUri);
					return Task.CompletedTask;
				};

				options.Events.OnRedirectToAccessDenied = context =>
				{
					if (context.Request.Path.StartsWithSegments("/api"))
					{
						context.Response.StatusCode = 403;
						return Task.CompletedTask;
					}
					context.Response.Redirect(context.RedirectUri);
					return Task.CompletedTask;
				};

				options.Events.OnRedirectToLogout = context =>
				{
					if (context.Request.Path.StartsWithSegments("/api"))
					{
						context.Response.StatusCode = 200;
						return Task.CompletedTask;
					}
					context.Response.Redirect(context.RedirectUri);
					return Task.CompletedTask;
				};
			});

			return services;
		}

		public static IServiceCollection AddCORSconfiguration(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("AllowSpecificOrigin", builder =>
				{
					builder.WithOrigins(
						"https://localhost:5173"
					)
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials() 
					.WithExposedHeaders(
						"Content-Length",
						"X-JSON-Response"
					);
				});
			});
			return services;
		}
	}
}
