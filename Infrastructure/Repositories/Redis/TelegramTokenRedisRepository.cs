using Application.Users.Interfaces;
using Infrastructure.Redis.Entities;
using StackExchange.Redis;

namespace Infrastructure.Repositories.Redis;

public class TelegramTokenRedisRepository(IConnectionMultiplexer redis) : RedisRepository<TelegramTokenEntity>(redis, "TgToken:"), ITelegramTokenRepository
{
	public async Task SetAsync(string token, int userId, TimeSpan expiry)
	{
		var entity = new TelegramTokenEntity { Id = token, UserId = userId };
		await SetAsync(entity, expiry);
	}

	public async Task<int?> GetUserIdAsync(string token)
	{
		var entity = await GetAsync(token);
		return entity?.UserId;
	}
}
