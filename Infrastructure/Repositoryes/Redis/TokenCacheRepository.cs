using Application.Intarfaces;
using StackExchange.Redis;

namespace Infrastructure.Repositoryes.Redis
{
    public class TokenCacheRepository : ITokenCacheRepository
    {
        private readonly IDatabase _db;

        public TokenCacheRepository(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SetAccessTokenAsync(int userId, string token, TimeSpan ttl)
        {
            await _db.StringSetAsync($"access:{userId}", token, ttl);
        }

        public async Task<string?> GetAccessTokenAsync(int userId)
        {
            return await _db.StringGetAsync($"access:{userId}");
        }

        public async Task RemoveAccessTokenAsync(int userId)
        {
            await _db.KeyDeleteAsync($"access:{userId}");
        }
    }
}
