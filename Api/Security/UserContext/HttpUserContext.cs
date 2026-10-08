using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;

namespace ViaTrade.Api.Security.UserContext;

public sealed class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
	public int UserId =>
		UserClaimsReader.FindUserId(httpContextAccessor.HttpContext?.User) ?? throw new AuthenticationException();

	public string SessionId =>
		UserClaimsReader.FindSessionId(httpContextAccessor.HttpContext?.User) ?? throw new AuthenticationException();
}
