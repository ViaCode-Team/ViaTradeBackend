using Application.Auth.Models;

namespace Application.Auth.Interfaces;

public interface IAuthCommandService
{
	Task<AuthTokens> LoginAsync(string login, string password, string userAgent, CancellationToken ct);
	Task LogoutAllAsync(int userId, CancellationToken ct);
	Task LogoutSessionAsync(string sessionId, CancellationToken ct);
	Task<AuthTokens> RefreshTokenAsync(string refreshToken, CancellationToken ct);
	Task<AuthTokens> RegisterAsync(string login, string password, string userAgent, CancellationToken ct);
}
