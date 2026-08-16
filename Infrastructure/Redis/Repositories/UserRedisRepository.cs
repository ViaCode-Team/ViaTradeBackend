using StackExchange.Redis;
using ViaTrade.Infrastructure.Redis.Entities;
using ViaTrade.Infrastructure.Redis.Keys;

namespace ViaTrade.Infrastructure.Redis.Repositories;

public class UserRedisRepository(IConnectionMultiplexer connectionMultiplexer)
	: BaseRedisRepository<UserRedisEntity>(connectionMultiplexer.GetDatabase(), RedisKeys.Cache.Users) { }
