using Application.Users.Interfaces;
using Infrastructure.Redis.Entities;
using StackExchange.Redis;

namespace Infrastructure.Repositories.Redis;

public class TgTokenRedisRepository(IConnectionMultiplexer redis) : RedisRepository<TgTokenEntity>(redis, "TgToken:"), ITgTokenRepository
{
	public async Task SetAsync(string token, int userId, TimeSpan expiry)
	{
		var entity = new TgTokenEntity { Id = token, UserId = userId };
		await SetAsync(entity, expiry);
	}

	public async Task<int?> GetUserIdAsync(string token)
	{
		var entity = await GetAsync(token);
		return entity?.UserId;
	}
}
