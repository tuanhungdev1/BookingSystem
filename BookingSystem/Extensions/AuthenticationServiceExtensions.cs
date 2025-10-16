using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
				options.IncludeErrorDetails = true;  // Thêm để debug
				options.MapInboundClaims = false;
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
					OnMessageReceived = context => {
						var token = context.Request.Cookies["accessToken"];
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
						logger.LogDebug("Token from cookie: {Token}", token ?? "NULL");  // Check nếu token null hoặc empty
						context.Token = token;
						return Task.CompletedTask;
					},
					OnAuthenticationFailed = context =>
					{
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
						logger.LogError("Authentication failed: {Error}", context.Exception.Message);
						return Task.CompletedTask;
					},
					OnTokenValidated = context => {
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
						logger.LogDebug("Claims: {Claims}", string.Join(", ", context.Principal.Claims.Select(c => $"{c.Type}={c.Value}")));
						return Task.CompletedTask;
					}
				};
			});

			// Thêm policy hỗ trợ cả schemes
			services.AddAuthorization(options =>
			{
				var multiSchemePolicy = new AuthorizationPolicyBuilder(
					IdentityConstants.ApplicationScheme,  // Cookie scheme từ Identity
					JwtBearerDefaults.AuthenticationScheme)
					.RequireAuthenticatedUser()
					.Build();
				options.DefaultPolicy = multiSchemePolicy;
			});

			return services;
		}
	}
}
