using BookingSystem.Application.Contracts;
using BookingSystem.Application.Models.Common;
using BookingSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BookingSystem.Application.Services
{
	public class JwtService : IJwtService
	{
		private readonly JwtSettings _jwtSettings;
		private readonly ILogger<JwtService> _logger;

		public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
		{
			_jwtSettings = jwtSettings.Value;
			_logger = logger;
		}
		public string GenerateAccessToken(User user, IList<string> roles)
		{
			try
			{
				_logger.LogDebug("Generating access token for user: {UserId}", user.Id);

				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
					new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
					new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(JwtRegisteredClaimNames.Iat,
						new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
						ClaimValueTypes.Integer64)
				};

				foreach (var role in roles)
				{
					claims.Add(new Claim(ClaimTypes.Role, role));
				}

				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
				var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

				var tokenDescriptor = new JwtSecurityToken(
					issuer: _jwtSettings.Issuer,
					audience: _jwtSettings.Audience,
					claims: claims,
					expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
					signingCredentials: creds
				);

				var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

				_logger.LogDebug("Access token generated successfully for user: {UserId}", user.Id);
				return token;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating access token for user: {UserId}", user.Id);
				throw;
			}
		}

		public string GenerateRefreshToken()
		{
			try
			{
				var randomNumber = new byte[64];
				using var rng = RandomNumberGenerator.Create();
				rng.GetBytes(randomNumber);
				var refreshToken = Convert.ToBase64String(randomNumber);

				_logger.LogDebug("Refresh token generated successfully");
				return refreshToken;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating refresh token");
				throw;
			}
		}
		public bool ValidateToken(string token)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(token))
				{
					_logger.LogWarning("Token validation failed: Token is null or empty");
					return false;
				}

				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

				var validationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = true,
					ValidIssuer = _jwtSettings.Issuer,
					ValidateAudience = true,
					ValidAudience = _jwtSettings.Audience,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.FromMinutes(2),
					RequireExpirationTime = true,
					RequireSignedTokens = true,
					ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
				};

				var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

				if (validatedToken is JwtSecurityToken jwtToken)
				{
					_logger.LogDebug("Token validation successful. Algorithm: {Alg}, Expiration: {Exp}",
						jwtToken.Header.Alg, jwtToken.ValidTo);
				}

				return true;
			}
			catch (SecurityTokenExpiredException ex)
			{
				_logger.LogWarning("Token validation failed - Token expired: {Message}", ex.Message);
				return false;
			}
			catch (SecurityTokenInvalidSignatureException ex)
			{
				_logger.LogWarning("Token validation failed - Invalid signature: {Message}", ex.Message);
				return false;
			}
			catch (SecurityTokenInvalidIssuerException ex)
			{
				_logger.LogWarning("Token validation failed - Invalid issuer: {Message}", ex.Message);
				return false;
			}
			catch (SecurityTokenInvalidAudienceException ex)
			{
				_logger.LogWarning("Token validation failed - Invalid audience: {Message}", ex.Message);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Token validation failed with unexpected error: {Message}", ex.Message);
				return false;
			}
		}
		public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
		{
			try
			{
				_logger.LogDebug("Validating expired token");

				var tokenValidationParameters = new TokenValidationParameters
				{
					ValidateAudience = true,
					ValidateIssuer = true,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
					ValidateLifetime = false,
					ValidIssuer = _jwtSettings.Issuer,
					ValidAudience = _jwtSettings.Audience,
					ClockSkew = TimeSpan.Zero,
					ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
				};

				var tokenHandler = new JwtSecurityTokenHandler();
				var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

				if (securityToken is not JwtSecurityToken jwtSecurityToken ||
					!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				{
					throw new SecurityTokenException("Invalid token algorithm");
				}

				return principal;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error validating expired token");
				throw;
			}
		}
	}
}
