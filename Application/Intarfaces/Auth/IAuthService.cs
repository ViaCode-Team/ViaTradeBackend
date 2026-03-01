using Domain.Models;

namespace Application.Intarfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string login, string password, string userAgent, CancellationToken cancellationToken);
        Task<AuthResult> RegisterAsync(string login, string password, CancellationToken cancellationToken);
        Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task LogoutSessionAsync(string refreshToken);
        Task LogoutAllAsync(int userId);
        Task<IEnumerable<UserSession>> GetUserSessionsAsync(int userId);
    }
}
