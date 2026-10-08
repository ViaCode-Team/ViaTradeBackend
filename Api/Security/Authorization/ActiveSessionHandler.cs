using Microsoft.AspNetCore.Authorization;
using ViaTrade.Api.Security.UserContext;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;

namespace ViaTrade.Api.Security.Authorization;

public class ActiveSessionRequirement : IAuthorizationRequirement { }

public class ActiveSessionHandler(ISessionRepository sessionRepository) : AuthorizationHandler<ActiveSessionRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		ActiveSessionRequirement requirement
	)
	{
		int? userId;
		string? sessionId;
		try
		{
			userId = UserClaimsReader.FindUserId(context.User);
			sessionId = UserClaimsReader.FindSessionId(context.User);
		}
		catch (InvalidTokenException)
		{
			context.Fail();
			return;
		}

		if (userId == null || sessionId == null)
		{
			context.Fail();
			return;
		}

		var session = await sessionRepository.FindByIdAsync(sessionId);
		if (session == null || session.UserId != userId || session.ExpiresAt <= DateTime.UtcNow)
		{
			context.Fail();
			return;
		}

		context.Succeed(requirement);
	}
}
