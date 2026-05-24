using System.Text.Json;
using Application.Interfaces.Redis;
using Domain.Models.Dto.User;
using StackExchange.Redis;

namespace Infrastructure.Repositoryes.Redis
{
    public class SessionRepository(
        IConnectionMultiplexer redis) : ISessionRepository
    {
        private readonly IDatabase _db = redis.GetDatabase();

        private static string SessionKey(string sessionId) => $"session:{sessionId}";
        private static string UserSessionsKey(int userId) => $"user:sessions:{userId}";

        public async Task CreateAsync(UserSession session, TimeSpan ttl)
        {
            var json = JsonSerializer.Serialize(session);

            var tran = _db.CreateTransaction();
            var setSession = tran.StringSetAsync(SessionKey(session.Id), json, ttl);
            var addToUser = tran.SortedSetAddAsync(UserSessionsKey(session.UserId), session.Id, session.CreatedAt.Ticks);

            bool committed = await tran.ExecuteAsync();
            if (!committed)
                throw new Exception("Failed to create session in Redis.");
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
            var removeFromUser = tran.SortedSetRemoveAsync(UserSessionsKey(session.UserId), sessionId);

            bool committed = await tran.ExecuteAsync();
            if (!committed)
                throw new Exception("Failed to remove session in Redis.");
        }

        public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(int userId)
        {
            var sessionIds = await _db.SortedSetRangeByRankAsync(UserSessionsKey(userId), 0, -1);
            var result = new List<UserSession>();

            foreach (var id in sessionIds)
            {
                var session = await GetAsync(id!);
                if (session != null)
                    result.Add(session);
            }

            return result;
        }

        // Cleaning old records from User`s Session SET <user:sessions:{userId}>
        public async Task<int> CleanupExpiredSessionsAsync(DateTime threshold)
        {
            var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
            var userKeys = server.Keys(pattern: "user:sessions:*", pageSize: 1000);

            int totalDeleted = 0;
            var thresholdTicks = threshold.Ticks;

            foreach (var userKey in userKeys)
            {
                var expiredSessionIds = await _db.SortedSetRangeByScoreAsync(
                    userKey,
                    double.NegativeInfinity,
                    thresholdTicks);

                if (expiredSessionIds.Length == 0) continue;

                foreach (var sessionId in expiredSessionIds)
                {
                    if (string.IsNullOrEmpty(sessionId)) continue;

                    try
                    {
                        var sessionValue = await _db.StringGetAsync(SessionKey(sessionId!));
                        if (!sessionValue.IsNullOrEmpty)
                        {
                            await _db.KeyDeleteAsync(SessionKey(sessionId!));
                        }

                        await _db.SortedSetRemoveAsync(userKey, sessionId!);

                        totalDeleted++;
                    }
                    catch (Exception ex)
                    {
                        // Log warning: $"Failed to cleanup session {sessionId}: {ex.Message}"
                    }
                }
            }

            return totalDeleted;
        }

        public IEnumerable<int> GetAllUserIdsWithSessions()
        {
            var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
            var keys = server.Keys(pattern: "user:sessions:*", pageSize: 10000);

            var ids = new List<int>();
            foreach (var key in keys)
            {
                if (int.TryParse(key.ToString().Split(':').Last(), out var userId))
                    ids.Add(userId);
            }
            return ids;
        }
    }
}