using Domain.Entities.Redis;
using StackExchange.Redis;

namespace Infrastructure.Repositoryes.Redis
{
    public class UserRedisRepository : RedisRepository<UserRedisEntity>
    {
        public UserRedisRepository(IConnectionMultiplexer redis)
            : base(redis, "User:") { }
    }

    public class UserRedisEntity : RedisEntity
    {
        public string Login { get; set; } = default!;
        public string? RefreshToken { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
