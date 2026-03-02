using Domain.Entities.Redis;
using StackExchange.Redis;

namespace Infrastructure.Repositoryes.Redis
{
    public class UserRedisRepository(IConnectionMultiplexer redis) : RedisRepository<UserRedisEntity>(redis, "User:")
    {
    }
}
