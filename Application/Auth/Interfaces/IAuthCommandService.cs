using Application.Common.Models;

namespace Application.Auth.Interfaces;

public interface IAuthCommandService
{
	Task<AuthInternalResult> LoginAsync(string login, string password, string userAgent, CancellationToken ct);
	Task LogoutAllAsync(int userId, CancellationToken ct);
	Task LogoutSessionAsync(string refreshToken, CancellationToken ct);
	Task<AuthInternalResult> RefreshTokenAsync(string refreshToken, CancellationToken ct);
	Task<AuthInternalResult> RegisterAsync(string login, string password, string userAgent, CancellationToken ct);
}
