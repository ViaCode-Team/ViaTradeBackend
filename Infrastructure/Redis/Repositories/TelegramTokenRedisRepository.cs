using Application.Users.Interfaces;
using Infrastructure.Redis.Entities;
using Infrastructure.Redis.Keys;
using StackExchange.Redis;

namespace Infrastructure.Redis.Repositories;

public class TelegramTokenRedisRepository(IConnectionMultiplexer connectionMultiplexer)
	: BaseRedisRepository<TelegramTokenEntity>(connectionMultiplexer.GetDatabase(), RedisKeys.Cache.TelegramTokens),
		ITelegramTokenRepository
{
	public async Task SetAsync(string token, int userId, TimeSpan expiry)
	{
		var entity = new TelegramTokenEntity { Id = token, UserId = userId };
		await SetAsync(entity, expiry);
	}

	public async Task<int?> FindUserIdAsync(string token)
	{
		var entity = await FindByIdAsync(token);
		return entity?.UserId;
	}
}
