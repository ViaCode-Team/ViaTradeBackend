using Domain.Entities.Redis;
using StackExchange.Redis;

namespace Infrastructure.Repositories.Redis
{
    public class TgTokenRepository(IConnectionMultiplexer redis) : RedisRepository<TgTokenEntity>(redis, "TgToken:")
    {
    }
}
