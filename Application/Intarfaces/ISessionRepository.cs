using Domain.Models;

namespace Application.Intarfaces
{
    public interface ISessionRepository
    {
        Task CreateAsync(UserSession session, TimeSpan ttl);
        Task<UserSession?> GetAsync(string sessionId);
        Task RemoveAsync(string sessionId);
        Task<IEnumerable<UserSession>> GetUserSessionsAsync(int userId);
    }

}
