using System.Text.Json;
using StackExchange.Redis;
using ViaTrade.Application.Users.Interfaces;
using ViaTrade.Infrastructure.Redis.Entities;
using ViaTrade.Infrastructure.Redis.Keys;
using ViaTrade.Infrastructure.Redis.Serialization;

namespace ViaTrade.Infrastructure.Redis.Repositories;

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

	public async Task<int?> ConsumeUserIdAsync(string token)
	{
		var value = await _database.StringGetDeleteAsync(GetKey(token));
		if (value.IsNullOrEmpty)
			return null;

		var entity = JsonSerializer.Deserialize(
			value.ToString(),
			RedisJsonSerializerContext.Default.TelegramTokenEntity
		);
		return entity?.UserId;
	}
}
