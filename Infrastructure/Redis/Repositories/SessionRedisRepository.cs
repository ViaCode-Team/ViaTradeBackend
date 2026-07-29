using System.Text.Json;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Users.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Redis.Repositories;

public class SessionRedisRepository(IConnectionMultiplexer redis, ILogger<SessionRedisRepository> logger)
	: ISessionRepository
{
	private const int CleanupBatchSize = 1_000;
	private readonly IDatabase _db = redis.GetDatabase();

	private static string SessionKey(string sessionId) => $"session:{sessionId}";

	private static string UserSessionsKey(int userId) => $"user:sessions:{userId}";

	private static RedisKey SessionExpirationsKey => "sessions:expires";

	private static string ExpirationMember(UserSessionDto session) => $"{session.UserId}:{session.Id}";

	public async Task CreateAsync(UserSessionDto session, TimeSpan ttl)
	{
		var json = JsonSerializer.Serialize(session);

		var tran = _db.CreateTransaction();
		var setSession = tran.StringSetAsync(SessionKey(session.Id), json, ttl);
		var addToUser = tran.SortedSetAddAsync(UserSessionsKey(session.UserId), session.Id, session.CreatedAt.Ticks);
		var addExpiration = tran.SortedSetAddAsync(
			SessionExpirationsKey,
			ExpirationMember(session),
			session.ExpiresAt.Ticks
		);

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
		var removeExpiration = tran.SortedSetRemoveAsync(SessionExpirationsKey, ExpirationMember(session));

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

		return new PageResult<UserSessionDto>(result, (int)totalCount, pageOptions.Page, pageOptions.PageSize);
	}

	public async Task<int> CleanupExpiredSessionsAsync(DateTime utcNow)
	{
		int totalDeleted = 0;
		var expiredSessions = await _db.SortedSetRangeByScoreWithScoresAsync(
			SessionExpirationsKey,
			double.NegativeInfinity,
			utcNow.Ticks,
			Exclude.None,
			Order.Ascending,
			0,
			CleanupBatchSize
		);

		foreach (var expiredSession in expiredSessions)
		{
			if (!TryParseExpirationMember(expiredSession.Element, out var userId, out var sessionId))
			{
				await _db.SortedSetRemoveAsync(SessionExpirationsKey, expiredSession.Element);
				totalDeleted++;
				continue;
			}

			try
			{
				var transaction = _db.CreateTransaction();
				transaction.AddCondition(Condition.KeyNotExists(SessionKey(sessionId)));
				transaction.AddCondition(
					Condition.SortedSetEqual(SessionExpirationsKey, expiredSession.Element, expiredSession.Score)
				);
				var removeExpiration = transaction.SortedSetRemoveAsync(SessionExpirationsKey, expiredSession.Element);
				var removeFromUser = transaction.SortedSetRemoveAsync(UserSessionsKey(userId), sessionId);

				if (await transaction.ExecuteAsync())
					totalDeleted++;
			}
			catch (Exception exception)
			{
				logger.LogWarning(
					exception,
					"Unable to remove expired session index: SessionId={SessionId}, UserId={UserId}",
					sessionId,
					userId
				);
			}
		}

		return totalDeleted;
	}

	private static bool TryParseExpirationMember(RedisValue value, out int userId, out string sessionId)
	{
		var parts = value.ToString().Split(':', 2);
		if (parts.Length == 2 && int.TryParse(parts[0], out userId) && !string.IsNullOrWhiteSpace(parts[1]))
		{
			sessionId = parts[1];
			return true;
		}

		userId = default;
		sessionId = string.Empty;
		return false;
	}
}
