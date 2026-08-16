using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Users.Models;

namespace ViaTrade.Application.Auth.Interfaces;

public interface ISessionRepository
{
	Task CreateSessionAsync(UserSessionDto session, string refreshToken, TimeSpan ttl);
	Task<UserSessionDto?> FindByIdAsync(string sessionId);
	Task<UserSessionDto?> FindByRefreshTokenAsync(string refreshToken);
	Task<bool> TryTerminateSessionByUsedRefreshTokenAsync(string refreshToken);
	Task<bool> TryRotateRefreshAsync(
		UserSessionDto session,
		string refreshToken,
		string newRefreshToken,
		TimeSpan sessionTtl,
		TimeSpan usedRefreshTokenTtl
	);
	Task TerminateSessionAsync(string sessionId);
	Task<IReadOnlyList<UserSessionDto>> ListByUserAsync(int userId);
	Task<PageResult<UserSessionDto>> GetPageByUserAsync(int userId, PageOptions pageOptions);
	Task<int> CleanupExpiredSessionsAsync(DateTime utcNow);
}
