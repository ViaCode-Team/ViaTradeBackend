using Application.Interfaces.Repositories.Redis;
using Domain.Entities.Redis;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Repositories.Redis
{
    public class RedisRepository<T>(IConnectionMultiplexer redis, string prefix) : IRedisRepository<T> where T : RedisEntity
    {
        protected readonly IDatabase _db = redis.GetDatabase();
        protected readonly string _prefix = prefix;

        protected string GetKey(string id) => $"{_prefix}{id}";

        public async Task<T?> GetAsync(string id)
        {
            var value = await _db.StringGetAsync(GetKey(id));
            if (value.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(value.ToString());
        }

        public async Task SetAsync(T entity, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(entity);
            await _db.StringSetAsync(GetKey(entity.Id), json, (Expiration)expiry!);
        }

        public async Task RemoveAsync(string id)
        {
            await _db.KeyDeleteAsync(GetKey(id));
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
            var keys = server.Keys(pattern: $"{_prefix}*");

            var result = new List<T>();
            foreach (var key in keys)
            {
                var value = await _db.StringGetAsync(key);

                if (!value.IsNullOrEmpty)
                    result.Add(JsonSerializer.Deserialize<T>(value.ToString())!);
            }

            return result;
        }
    }
}
