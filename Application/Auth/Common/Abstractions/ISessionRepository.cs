namespace ViaTrade.Application.Auth.Common.Abstractions;

public interface ISessionRepository
{
	Task CreateSessionAsync(SessionData session, string refreshToken, TimeSpan ttl);
	Task<SessionData?> FindByIdAsync(string sessionId);
	Task<SessionData?> FindByRefreshTokenAsync(string refreshToken);
	Task<bool> TryTerminateSessionByUsedRefreshTokenAsync(string refreshToken);
	Task<bool> TryRotateRefreshAsync(
		SessionData session,
		string refreshToken,
		string newRefreshToken,
		TimeSpan sessionTtl,
		TimeSpan usedRefreshTokenTtl
	);
	Task TerminateSessionAsync(string sessionId);
	Task<IReadOnlyList<SessionData>> ListByUserAsync(int userId);
	Task<int> CleanupExpiredSessionsAsync(DateTime utcNow);
}
