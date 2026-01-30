using Application.Intarfaces;
using StackExchange.Redis;

namespace Infrastructure.Repositoryes.Redis
{
    public class RefreshTokenRepository(IConnectionMultiplexer redis) : IRefreshTokenRepository
    {
        private readonly IDatabase _db = redis.GetDatabase();

        private static string TokenKey(string sessionId) => $"refresh:{sessionId}";
        private static string IndexKey(string token) => $"refresh:idx:{token}";

        public async Task StoreAsync(string sessionId, string refreshToken)
        {
            var tran = _db.CreateTransaction();
            var setToken = tran.StringSetAsync(TokenKey(sessionId), refreshToken);
            var setIndex = tran.StringSetAsync(IndexKey(refreshToken), sessionId);

            bool committed = await tran.ExecuteAsync();
            if (!committed)
                throw new Exception("Failed to store refresh token in Redis.");

            await Task.WhenAll(setToken, setIndex);
        }

        public async Task<string?> GetSessionIdAsync(string refreshToken)
        {
            return await _db.StringGetAsync(IndexKey(refreshToken));
        }

        public async Task RotateAsync(string sessionId, string newRefreshToken)
        {
            var oldToken = await _db.StringGetAsync(TokenKey(sessionId));
            if (!oldToken.IsNullOrEmpty)
                await _db.KeyDeleteAsync(IndexKey(oldToken!));

            var tran = _db.CreateTransaction();
            var setToken = tran.StringSetAsync(TokenKey(sessionId), newRefreshToken);
            var setIndex = tran.StringSetAsync(IndexKey(newRefreshToken), sessionId);

            bool committed = await tran.ExecuteAsync();
            if (!committed)
                throw new Exception("Failed to rotate refresh token in Redis.");

            await Task.WhenAll(setToken, setIndex);
        }

        public async Task RemoveAsync(string sessionId)
        {
            var token = await _db.StringGetAsync(TokenKey(sessionId));
            if (!token.IsNullOrEmpty)
                await _db.KeyDeleteAsync(IndexKey(token!));

            await _db.KeyDeleteAsync(TokenKey(sessionId));
        }
    }
}
