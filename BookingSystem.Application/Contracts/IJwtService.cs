using BookingSystem.Domain.Entities;
using System.Security.Claims;

namespace BookingSystem.Application.Contracts
{
	public interface IJwtService
	{
		string GenerateAccessToken(User user, IList<string> roles);
		string GenerateRefreshToken();
		ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
		bool ValidateToken(string token);
	}
}
