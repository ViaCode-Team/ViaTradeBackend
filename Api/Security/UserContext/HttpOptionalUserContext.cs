using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Api.Security.UserContext;

public sealed class HttpOptionalUserContext(IHttpContextAccessor httpContextAccessor) : IOptionalUserContext
{
	public int? UserId => UserClaimsReader.FindUserId(httpContextAccessor.HttpContext?.User);

	public string? SessionId => UserClaimsReader.FindSessionId(httpContextAccessor.HttpContext?.User);
}
