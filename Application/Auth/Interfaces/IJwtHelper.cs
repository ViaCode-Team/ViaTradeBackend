using Domain.Users.Entities;
using System.Security.Claims;

namespace Application.Auth.Interfaces;

public interface IJwtHelper
{
	string GenerateAccessToken(User user, string sessionId);
	string GenerateRefreshToken();
	string GetSessionId(ClaimsPrincipal user);
	int GetUserIdFromClaims(ClaimsPrincipal user);
}
