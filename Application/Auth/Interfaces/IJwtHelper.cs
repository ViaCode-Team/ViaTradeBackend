using System.Security.Claims;
using Application.Users.Models;

namespace Application.Auth.Interfaces;

public interface IJwtHelper
{
	string GenerateAccessToken(UserTokenDto user, string sessionId, DateTime expiresAt);
	string GenerateRefreshToken();
	string GetSessionId(ClaimsPrincipal user);
	int GetUserIdFromClaims(ClaimsPrincipal user);
}
