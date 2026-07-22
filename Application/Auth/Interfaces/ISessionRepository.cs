using Application.Common.Models;
using Application.Users.Models;

namespace Application.Auth.Interfaces;

public interface ISessionRepository
{
	Task CreateAsync(UserSessionDto session, TimeSpan ttl);
	Task<UserSessionDto?> FindByIdAsync(string sessionId);
	Task RemoveAsync(string sessionId);
	Task<IReadOnlyList<UserSessionDto>> ListByUserAsync(int userId);
	Task<PageResult<UserSessionDto>> GetPageByUserAsync(int userId, PageOptions pageOptions);
	IReadOnlyList<int> ListUserIds();
	Task<int> CleanupExpiredSessionsAsync(DateTime threshold);
}
