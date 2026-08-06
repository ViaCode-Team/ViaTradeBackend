using Infrastructure.Redis.Entities;
using Infrastructure.Redis.Keys;
using StackExchange.Redis;

namespace Infrastructure.Redis.Repositories;

public class UserRedisRepository(IConnectionMultiplexer connectionMultiplexer)
	: BaseRedisRepository<UserRedisEntity>(connectionMultiplexer.GetDatabase(), RedisKeys.Cache.Users) { }
