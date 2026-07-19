using System.Security.Claims;
using Domain.Users.Entities;

namespace Application.Auth.Interfaces;

public interface IJwtHelper
{
	string GenerateAccessToken(User user, string sessionId);
	string GenerateRefreshToken();
	string GetSessionId(ClaimsPrincipal user);
	int GetUserIdFromClaims(ClaimsPrincipal user);
}
