using Infrastructure.Redis.Entities;
using StackExchange.Redis;

namespace Infrastructure.Redis.Repositories;

public class UserRedisRepository(IConnectionMultiplexer redis) : RedisRepository<UserRedisEntity>(redis, "User:") { }
