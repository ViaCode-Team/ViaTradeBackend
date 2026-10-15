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

	Task<SessionRevocationPage> ExecuteTerminateStalePageAsync(
		int userId,
		long authorizationVersion,
		int offset,
		int limit
	);

	Task<int> CleanupExpiredSessionsAsync(DateTime utcNow);
}
