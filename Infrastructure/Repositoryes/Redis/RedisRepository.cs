using System.Text.Json;
using Application.Intarfaces;
using Domain.Entities.Redis;
using StackExchange.Redis;

namespace Infrastructure.Repositoryes.Redis
{
    public class RedisRepository<T> : IRedisRepository<T> where T : RedisEntity
    {
        protected readonly StackExchange.Redis.IDatabase _db;
        protected readonly string _prefix;

        public RedisRepository(IConnectionMultiplexer redis, string prefix)
        {
            _db = redis.GetDatabase();
            _prefix = prefix;
        }

        protected string GetKey(string id) => $"{_prefix}{id}";

        public async Task<T?> GetAsync(string id)
        {
            var value = await _db.StringGetAsync(GetKey(id));
            if (value.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetAsync(T entity, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(entity);
            await _db.StringSetAsync(GetKey(entity.Id), json, (Expiration)expiry);
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
                    result.Add(JsonSerializer.Deserialize<T>(value)!);
            }

            return result;
        }
    }
}
