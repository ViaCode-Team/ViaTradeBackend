using System.Security.Claims;

namespace ViaTrade.Application.Auth.Common.Abstractions;

public interface IJwtHelper
{
	string GenerateAccessToken(TokenUser user, string sessionId, DateTime expiresAt);
	string GenerateRefreshToken();
	string GetSessionId(ClaimsPrincipal user);
	int GetUserIdFromClaims(ClaimsPrincipal user);
}
