using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookingSystem.Extensions
{
	public class CustomRolesAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
	{
		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			RolesAuthorizationRequirement requirement)
		{
			// Lấy tất cả role claims
			var roleClaims = context.User.FindAll(ClaimTypes.Role).ToList();
			var userRoles = roleClaims.Select(c => c.Value).ToList();

			// Kiểm tra nếu user có role nào trong yêu cầu
			if (requirement.AllowedRoles.Any(role => userRoles.Contains(role)))
			{
				context.Succeed(requirement);
			}
			else
			{
				context.Fail();
			}

			return Task.CompletedTask;
		}
	}
}
