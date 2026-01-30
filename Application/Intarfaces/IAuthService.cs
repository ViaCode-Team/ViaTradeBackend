using Domain.Models;

namespace Application.Intarfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string login, string password, string clientId, string userAgent);
        Task<AuthResult> RegisterAsync(string login, string password);
        Task<AuthResult> RefreshTokenAsync(string refreshToken, string clientId);
        Task LogoutSessionAsync(string refreshToken);
        Task LogoutAllAsync(int userId);
        Task<IEnumerable<UserSession>> GetUserSessionsAsync(int userId);
    }
}
