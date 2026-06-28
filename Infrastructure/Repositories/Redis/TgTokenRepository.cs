using Domain.Entities.Redis;
using Infrastructure.Repositories.Redis;
using StackExchange.Redis;

namespace Infrastructure.Repositories.Redis
{
    public class TgTokenRepository(IConnectionMultiplexer redis) : RedisRepository<TgTokenEntity>(redis, "TgToken:")
    {

    }
}
