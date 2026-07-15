using Domain.Models.Dto.User;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Redis;

public interface ISessionRepository
{
	Task CreateAsync(UserSession session, TimeSpan ttl);
	Task<UserSession?> GetAsync(string sessionId);
	Task RemoveAsync(string sessionId);
	Task<IEnumerable<UserSession>> GetUserSessionsAsync(int userId);
	Task<PagedResult<UserSession>> GetPagedUserSessionsAsync(int userId, PaginationRequest paginationRequest);
	IEnumerable<int> GetAllUserIdsWithSessions();
	Task<int> CleanupExpiredSessionsAsync(DateTime threshold);
}
