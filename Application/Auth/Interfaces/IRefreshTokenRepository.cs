namespace Application.Auth.Interfaces;

public interface IRefreshTokenRepository
{
	Task StoreAsync(string sessionId, string refreshToken, TimeSpan ttl);
	Task<string?> GetSessionIdAsync(string refreshToken);
	Task RotateAsync(string sessionId, string newRefreshToken, TimeSpan ttl);
	Task RemoveAsync(string sessionId);
}
