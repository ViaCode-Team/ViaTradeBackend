using Application.Common.Models.Pagination;
using Application.Users.Models;

namespace Application.Auth.Interfaces;

public interface ISessionRepository
{
	Task CreateAsync(UserSessionDto session, TimeSpan ttl);
	Task<UserSessionDto?> GetAsync(string sessionId);
	Task RemoveAsync(string sessionId);
	Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(int userId);
	Task<PagedResult<UserSessionDto>> GetPagedUserSessionsAsync(int userId, PaginationRequest paginationRequest);
	IEnumerable<int> GetAllUserIdsWithSessions();
	Task<int> CleanupExpiredSessionsAsync(DateTime threshold);
}
