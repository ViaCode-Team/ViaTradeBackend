using System.Text.Json;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Users.Models;
using StackExchange.Redis;

namespace Infrastructure.Redis.Repositories;

public class SessionRedisRepository(IConnectionMultiplexer redis) : ISessionRepository
{
	private readonly IDatabase _db = redis.GetDatabase();

	private static string SessionKey(string sessionId) => $"session:{sessionId}";

	private static string UserSessionsKey(int userId) => $"user:sessions:{userId}";

	public async Task CreateAsync(UserSessionDto session, TimeSpan ttl)
	{
		var json = JsonSerializer.Serialize(session);

		var tran = _db.CreateTransaction();
		var setSession = tran.StringSetAsync(SessionKey(session.Id), json, ttl);
		var addToUser = tran.SortedSetAddAsync(UserSessionsKey(session.UserId), session.Id, session.CreatedAt.Ticks);

		bool committed = await tran.ExecuteAsync();
		if (!committed)
			throw new Exception("Failed to create session in Redis.");
	}

	public async Task<UserSessionDto?> FindByIdAsync(string sessionId)
	{
		var value = await _db.StringGetAsync(SessionKey(sessionId));
		if (value.IsNullOrEmpty)
			return null;

		return JsonSerializer.Deserialize<UserSessionDto>(value.ToString());
	}

	public async Task RemoveAsync(string sessionId)
	{
		var session = await FindByIdAsync(sessionId);
		if (session == null)
			return;

		var tran = _db.CreateTransaction();
		var delSession = tran.KeyDeleteAsync(SessionKey(sessionId));
		var removeFromUser = tran.SortedSetRemoveAsync(UserSessionsKey(session.UserId), sessionId);

		bool committed = await tran.ExecuteAsync();
		if (!committed)
			throw new Exception("Failed to remove session in Redis.");
	}

	public async Task<IReadOnlyList<UserSessionDto>> ListByUserAsync(int userId)
	{
		var sessionIds = await _db.SortedSetRangeByRankAsync(UserSessionsKey(userId), 0, -1);
		List<UserSessionDto> result = [];
		if (sessionIds.Length == 0)
			return result;

		var keys = sessionIds
			.Where(id => !id.IsNullOrEmpty)
			.Select(id => (RedisKey)SessionKey(id.ToString()))
			.ToArray();
		var values = await _db.StringGetAsync(keys);

		foreach (var value in values)
		{
			if (!value.IsNullOrEmpty)
			{
				var item = JsonSerializer.Deserialize<UserSessionDto>(value.ToString());
				if (item != null)
				{
					result.Add(item);
				}
			}
		}

		return result;
	}

	public async Task<PageResult<UserSessionDto>> GetPageByUserAsync(int userId, PageOptions pageOptions)
	{
		var totalCount = await _db.SortedSetLengthAsync(UserSessionsKey(userId));

		int start = (pageOptions.Page - 1) * pageOptions.PageSize;
		int stop = start + pageOptions.PageSize - 1;

		var sessionIds = await _db.SortedSetRangeByRankAsync(UserSessionsKey(userId), start, stop, Order.Descending);
		List<UserSessionDto> result = [];

		if (sessionIds.Length > 0)
		{
			var keys = sessionIds
				.Where(id => !id.IsNullOrEmpty)
				.Select(id => (RedisKey)SessionKey(id.ToString()))
				.ToArray();
			var values = await _db.StringGetAsync(keys);

			foreach (var value in values)
			{
				if (!value.IsNullOrEmpty)
				{
					var item = JsonSerializer.Deserialize<UserSessionDto>(value.ToString());
					if (item != null)
					{
						result.Add(item);
					}
				}
			}
		}

		return new PageResult<UserSessionDto>(
			result,
			(int)totalCount,
			pageOptions.Page,
			pageOptions.PageSize
		);
	}

	// Cleaning old records from User`s Session SET <user:sessions:{userId}>

	public async Task<int> CleanupExpiredSessionsAsync(DateTime threshold)
	{
		var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
		var userKeys = server.Keys(pattern: "user:sessions:*", pageSize: 1000);

		int totalDeleted = 0;
		var thresholdTicks = threshold.Ticks;

		foreach (var userKey in userKeys)
		{
			var expiredSessionIds = await _db.SortedSetRangeByScoreAsync(
				userKey,
				double.NegativeInfinity,
				thresholdTicks
			);

			if (expiredSessionIds.Length == 0)
				continue;

			foreach (var sessionId in expiredSessionIds)
			{
				if (string.IsNullOrEmpty(sessionId))
					continue;

				try
				{
					string idString = sessionId.ToString();
					var sessionValue = await _db.StringGetAsync(SessionKey(idString));
					if (!sessionValue.IsNullOrEmpty)
					{
						await _db.KeyDeleteAsync(SessionKey(idString));
					}

					await _db.SortedSetRemoveAsync(userKey, sessionId);

					totalDeleted++;
				}
				catch
				{
					// Log warning silently
				}
			}
		}

		return totalDeleted;
	}

	public IReadOnlyList<int> ListUserIds()
	{
		var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
		var keys = server.Keys(pattern: "user:sessions:*", pageSize: 10000);

		List<int> ids = [];
		foreach (var key in keys)
		{
			if (int.TryParse(key.ToString().Split(':').Last(), out var userId))
				ids.Add(userId);
		}
		return ids;
	}
}
