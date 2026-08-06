using System.Text.Json;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Infrastructure.Redis.Keys;
using StackExchange.Redis;

namespace Infrastructure.Redis.Repositories;

public class BaseRedisRepository<TEntity>(IDatabase database, RedisKeyBuilder keys) : ICacheRepository<TEntity>
	where TEntity : CacheEntity
{
	protected readonly IDatabase _database = database;
	private readonly RedisKeyBuilder _keys = keys;

	protected RedisKey GetKey(string id) => _keys.Build(id);

	public async Task<TEntity?> FindByIdAsync(string id)
	{
		var value = await _database.StringGetAsync(GetKey(id));
		if (value.IsNullOrEmpty)
			return default;
		return JsonSerializer.Deserialize<TEntity>(value.ToString());
	}

	public async Task SetAsync(TEntity entity, TimeSpan? expiry = null)
	{
		var json = JsonSerializer.Serialize(entity);
		if (expiry.HasValue)
		{
			await _database.StringSetAsync(GetKey(entity.Id), json, expiry.Value);
		}
		else
		{
			await _database.StringSetAsync(GetKey(entity.Id), json);
		}
	}

	public async Task RemoveAsync(string id)
	{
		await _database.KeyDeleteAsync(GetKey(id));
	}

	public async Task<IReadOnlyList<TEntity>> ListAsync()
	{
		var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints()[0]);
		var keys = server.Keys(pattern: _keys.Pattern);

		List<TEntity> result = [];
		foreach (var key in keys)
		{
			var value = await _database.StringGetAsync(key);

			if (!value.IsNullOrEmpty)
			{
				var item = JsonSerializer.Deserialize<TEntity>(value.ToString());
				if (item != null)
				{
					result.Add(item);
				}
			}
		}

		return result;
	}
}
