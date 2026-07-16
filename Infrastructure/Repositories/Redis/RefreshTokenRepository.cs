using Application.Interfaces.Repositories.Redis;
using StackExchange.Redis;

namespace Infrastructure.Repositories.Redis;

public class RefreshTokenRepository(IConnectionMultiplexer redis) : IRefreshTokenRepository
{
	private readonly IDatabase _db = redis.GetDatabase();

	private static string TokenKey(string sessionId) => $"refresh:{sessionId}";

	private static string IndexKey(string token) => $"refresh:idx:{token}";

	public async Task StoreAsync(string sessionId, string refreshToken, TimeSpan ttl)
	{
		var tran = _db.CreateTransaction();

		var setToken = tran.StringSetAsync(TokenKey(sessionId), refreshToken, ttl);
		var setIndex = tran.StringSetAsync(IndexKey(refreshToken), sessionId, ttl);

		bool committed = await tran.ExecuteAsync();
		if (!committed)
			throw new Exception("Failed to store refresh token in Redis.");
	}

	public async Task<string?> GetSessionIdAsync(string refreshToken)
	{
		return await _db.StringGetAsync(IndexKey(refreshToken));
	}

	public async Task RotateAsync(string sessionId, string newRefreshToken, TimeSpan ttl)
	{
		var oldToken = await _db.StringGetAsync(TokenKey(sessionId));

		// Cleanup old index key
		if (!oldToken.IsNullOrEmpty)
			await _db.KeyDeleteAsync(IndexKey(oldToken.ToString()));

		var tran = _db.CreateTransaction();
		var setToken = tran.StringSetAsync(TokenKey(sessionId), newRefreshToken, ttl);
		var setIndex = tran.StringSetAsync(IndexKey(newRefreshToken), sessionId, ttl);

		bool committed = await tran.ExecuteAsync();
		if (!committed)
			throw new Exception("Failed to rotate refresh token in Redis.");
	}

	public async Task RemoveAsync(string sessionId)
	{
		var token = await _db.StringGetAsync(TokenKey(sessionId));
		if (!token.IsNullOrEmpty)
			await _db.KeyDeleteAsync(IndexKey(token.ToString()));

		await _db.KeyDeleteAsync(TokenKey(sessionId));
	}
}
