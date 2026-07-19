using Infrastructure.Redis.Entities;
using StackExchange.Redis;

namespace Infrastructure.Repositories.Redis;

public class UserRedisRepository(IConnectionMultiplexer redis) : RedisRepository<UserRedisEntity>(redis, "User:") { }
