using System.Security.Claims;
using ViaTrade.Application.Users.Models;

namespace ViaTrade.Application.Auth.Interfaces;

public interface IJwtHelper
{
	string GenerateAccessToken(UserTokenDto user, string sessionId, DateTime expiresAt);
	string GenerateRefreshToken();
	string GetSessionId(ClaimsPrincipal user);
	int GetUserIdFromClaims(ClaimsPrincipal user);
}
