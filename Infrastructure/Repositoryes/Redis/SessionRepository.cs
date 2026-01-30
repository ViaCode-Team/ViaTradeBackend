using System.Text.Json;
using Application.Intarfaces;
using Domain.Models;
using StackExchange.Redis;

namespace Infrastructure.Repositoryes.Redis
{
    public class SessionRepository(IConnectionMultiplexer redis) : ISessionRepository
    {
        private readonly IDatabase _db = redis.GetDatabase();

        private static string SessionKey(string sessionId) => $"session:{sessionId}";
        private static string UserSessionsKey(int userId) => $"user:sessions:{userId}";

        public async Task CreateAsync(UserSession session, TimeSpan ttl)
        {
            var json = JsonSerializer.Serialize(session);

            var tran = _db.CreateTransaction();
            var setSession = tran.StringSetAsync(SessionKey(session.Id), json, ttl);
            var addToUser = tran.SetAddAsync(UserSessionsKey(session.UserId), session.Id);

            bool committed = await tran.ExecuteAsync();
            if (!committed)
                throw new Exception("Failed to create session in Redis.");

            await Task.WhenAll(setSession, addToUser);
        }

        public async Task<UserSession?> GetAsync(string sessionId)
        {
            var value = await _db.StringGetAsync(SessionKey(sessionId));
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<UserSession>(value!);
        }

        public async Task RemoveAsync(string sessionId)
        {
            var session = await GetAsync(sessionId);
            if (session == null) return;

            var tran = _db.CreateTransaction();
            var delSession = tran.KeyDeleteAsync(SessionKey(sessionId));
            var removeFromUser = tran.SetRemoveAsync(UserSessionsKey(session.UserId), sessionId);

            bool committed = await tran.ExecuteAsync();
            if (!committed)
                throw new Exception("Failed to remove session in Redis.");

            await Task.WhenAll(delSession, removeFromUser);
        }

        public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(int userId)
        {
            var sessionIds = await _db.SetMembersAsync(UserSessionsKey(userId));
            var result = new List<UserSession>();

            foreach (var id in sessionIds)
            {
                var session = await GetAsync(id!);
                if (session != null)
                    result.Add(session);
            }

            return result;
        }
    }
}
