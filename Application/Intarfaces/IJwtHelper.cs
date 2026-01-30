using System.Security.Claims;
using Domain.Entities.DataBase;

namespace Application.Intarfaces
{
    public interface IJwtHelper
    {
        string GenerateAccessToken(User user, string sessionId);
        string GenerateRefreshToken();
        string GetSessionId(ClaimsPrincipal user);
        int GetUserIdFromClaims(ClaimsPrincipal user);
    }
}
