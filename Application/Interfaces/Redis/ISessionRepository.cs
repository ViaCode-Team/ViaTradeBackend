using Domain.Models;

namespace Application.Interfaces.Redis
{
    public interface ISessionRepository
    {
        Task CreateAsync(UserSession session, TimeSpan ttl);
        Task<UserSession?> GetAsync(string sessionId);
        Task RemoveAsync(string sessionId);
        Task<IEnumerable<UserSession>> GetUserSessionsAsync(int userId);
        IEnumerable<int> GetAllUserIdsWithSessions();
        Task<int> CleanupExpiredSessionsAsync(DateTime threshold);
    }

}
