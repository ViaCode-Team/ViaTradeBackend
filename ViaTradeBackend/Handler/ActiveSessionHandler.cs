using System.IdentityModel.Tokens.Jwt;
using Application.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ViaTradeBackend.Handler;

public class ActiveSessionRequirement : IAuthorizationRequirement { }

public class ActiveSessionHandler(ISessionRepository sessionRepository) : AuthorizationHandler<ActiveSessionRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		ActiveSessionRequirement requirement
	)
	{
		var sessionId = context.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

		if (string.IsNullOrEmpty(sessionId))
		{
			context.Fail();
			return;
		}

		var session = await sessionRepository.FindByIdAsync(sessionId);
		if (session == null)
		{
			context.Fail();
			return;
		}

		context.Succeed(requirement);
	}
}
