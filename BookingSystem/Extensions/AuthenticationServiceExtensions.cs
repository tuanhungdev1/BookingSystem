using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace BookingSystem.Extensions
{
	public static class AuthenticationServiceExtensions
	{
		public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
		{

			// 1. Thêm Cookie Authentication trước
			services.ConfigureApplicationCookie(options =>
			{
				options.Cookie.Name = "BookingSystemAuth";
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				options.Cookie.SameSite = SameSiteMode.Strict;
				options.ExpireTimeSpan = TimeSpan.FromHours(1);
				options.SlidingExpiration = true;
				options.LoginPath = "/api/auth/login";
				options.AccessDeniedPath = "/api/auth/access-denied";
			});
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
		   .AddJwtBearer(options =>
			{
				options.SaveToken = false;
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
					NameClaimType = ClaimTypes.Name,
					RoleClaimType = ClaimTypes.Role,
					ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
				};

				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						var token = context.Request.Cookies["accessToken"];
						if (!string.IsNullOrEmpty(token))
						{
							context.Token = token;
						}
						return Task.CompletedTask;
					},
					OnAuthenticationFailed = context =>
					{
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
						logger.LogError("Authentication failed: {Error}", context.Exception.Message);
						return Task.CompletedTask;
					},
					OnTokenValidated = context =>
					{
						var roles = context.Principal?.FindAll(ClaimTypes.Role);
						var logger = context.HttpContext.RequestServices
							.GetRequiredService<ILogger<Program>>();
						logger.LogInformation("✓ Token validated - User: {User}, Roles: {Roles}",
							context.Principal?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
							string.Join(", ", roles?.Select(r => r.Value) ?? new List<string>()));
						// ✅ THÊM DÒNG NÀY: Đảm bảo ClaimsIdentity nhận role claims đúng
						if (context.Principal?.Identity is ClaimsIdentity identity)
						{
							var roles_list = context.Principal.FindAll(ClaimTypes.Role).ToList();
							if (roles_list.Count > 0)
							{
								logger.LogDebug("Found {RoleCount} roles", roles_list.Count);
							}
						}


						return Task.CompletedTask;
					}
				};
			});

			services.AddSingleton<IAuthorizationHandler, CustomRolesAuthorizationHandler>();

			// 3. Cấu hình Authorization policy
			services.AddAuthorization(options =>
			{
				// Default policy
				options.DefaultPolicy = new AuthorizationPolicyBuilder(
					JwtBearerDefaults.AuthenticationScheme)
					.RequireAuthenticatedUser()
					.Build();

				// ✅ Admin Policy - SỬ DỤNG RequireRole, KHÔNG dùng RequireClaim
				options.AddPolicy("Admin", policy =>
				{
					policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
					policy.RequireAuthenticatedUser();
					policy.RequireRole("Admin");  // ✅ ĐÚNG
				});

				// ✅ Host Policy
				options.AddPolicy("Host", policy =>
				{
					policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
					policy.RequireAuthenticatedUser();
					policy.RequireRole("Host");
				});

				// ✅ User Policy
				options.AddPolicy("User", policy =>
				{
					policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
					policy.RequireAuthenticatedUser();
					policy.RequireRole("User");
				});

				// ✅ AdminOrHost Policy
				options.AddPolicy("AdminOrHost", policy =>
				{
					policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
					policy.RequireAuthenticatedUser();
					policy.RequireRole("Admin", "Host");
				});
			});



			return services;
		}
	}
}
