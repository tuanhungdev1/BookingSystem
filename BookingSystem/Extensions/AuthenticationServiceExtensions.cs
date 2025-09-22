using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BookingSystem.Extensions
{
	public static class AuthenticationServiceExtensions
	{
		public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = configuration["JwtSettings:Issuer"],
					ValidateAudience = true,
					ValidAudience = configuration["JwtSettings:Audience"],
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!)),
					ClockSkew = TimeSpan.FromMinutes(2),
					RequireExpirationTime = true,
					RequireSignedTokens = true,
					ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
				};

				options.Events = new JwtBearerEvents
				{
					OnAuthenticationFailed = context =>
					{
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
						logger.LogError("Authentication failed: {Error}", context.Exception.Message);
						return Task.CompletedTask;
					},
					OnTokenValidated = context =>
					{
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
						logger.LogDebug("Token validated for user: {User}", context.Principal?.Identity?.Name);
						return Task.CompletedTask;
					}
				};
			});

			return services;
		}
	}
}
