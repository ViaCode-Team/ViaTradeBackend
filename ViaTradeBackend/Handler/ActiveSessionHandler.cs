using Application.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace ViaTradeBackend.Handler;

public class ActiveSessionRequirement : IAuthorizationRequirement { }

public class ActiveSessionHandler(ISessionRepository sessionRepository) : AuthorizationHandler<ActiveSessionRequirement>
{
	private readonly ISessionRepository _sessionRepository = sessionRepository;

	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		ActiveSessionRequirement requirement)
	{
		var sessionId = context.User.Claims
			.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
		if (string.IsNullOrEmpty(sessionId))
		{
			context.Fail();
			return;
		}

		var session = await _sessionRepository.GetAsync(sessionId);
		if (session == null)
		{
			context.Fail();
			return;
		}

		context.Succeed(requirement);
	}
}
