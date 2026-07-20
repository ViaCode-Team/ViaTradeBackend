using System.Text.Json;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using StackExchange.Redis;

namespace Infrastructure.Redis.Repositories;

public class RedisRepository<T>(IConnectionMultiplexer redis, string prefix) : ICacheRepository<T>
	where T : CacheEntity
{
	protected readonly IDatabase _db = redis.GetDatabase();
	protected readonly string _prefix = prefix;

	protected string GetKey(string id) => $"{_prefix}{id}";

	public async Task<T?> GetAsync(string id)
	{
		var value = await _db.StringGetAsync(GetKey(id));
		if (value.IsNullOrEmpty)
			return default;
		return JsonSerializer.Deserialize<T>(value.ToString());
	}

	public async Task SetAsync(T entity, TimeSpan? expiry = null)
	{
		var json = JsonSerializer.Serialize(entity);
		if (expiry.HasValue)
		{
			await _db.StringSetAsync(GetKey(entity.Id), json, expiry.Value);
		}
		else
		{
			await _db.StringSetAsync(GetKey(entity.Id), json);
		}
	}

	public async Task RemoveAsync(string id)
	{
		await _db.KeyDeleteAsync(GetKey(id));
	}

	public async Task<IEnumerable<T>> GetAllAsync()
	{
		var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
		var keys = server.Keys(pattern: $"{_prefix}*");

		List<T> result = [];
		foreach (var key in keys)
		{
			var value = await _db.StringGetAsync(key);

			if (!value.IsNullOrEmpty)
			{
				var item = JsonSerializer.Deserialize<T>(value.ToString());
				if (item != null)
				{
					result.Add(item);
				}
			}
		}

		return result;
	}
}
