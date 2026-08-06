using Application.Users.Models;
using Infrastructure.Redis.Keys;
using Infrastructure.Redis.Scripts;
using StackExchange.Redis;

namespace Infrastructure.Redis.Utils;

internal sealed class SessionRedisStorageHelper(IDatabase database)
{
	private readonly IDatabase _database = database;

	public async Task<bool> TryCreateAsync(UserSessionDto session, string sessionJson, string refreshTokenFingerprint, TimeSpan ttl)
	{
		var transaction = _database.CreateTransaction();
		var setSession = transaction.StringSetAsync(RedisKeys.Sessions.ById(session.Id), sessionJson, ttl);
		var setRefreshTokenFingerprint = transaction.StringSetAsync(
			RedisKeys.Sessions.RefreshTokenFingerprint(session.Id),
			refreshTokenFingerprint,
			ttl
		);
		var setRefreshTokenIndex = transaction.StringSetAsync(
			RedisKeys.Sessions.RefreshTokenIndex(refreshTokenFingerprint),
			session.Id,
			ttl
		);
		var addToUser = transaction.SortedSetAddAsync(
			RedisKeys.Sessions.ByUser(session.UserId),
			session.Id,
			session.CreatedAt.Ticks
		);
		var addExpiration = transaction.SortedSetAddAsync(
			RedisKeys.Sessions.ExpirationIndex,
			RedisKeys.Sessions.ExpirationMember(session.UserId, session.Id),
			session.ExpiresAt.Ticks
		);

		if (!await transaction.ExecuteAsync())
			return false;

		await Task.WhenAll(setSession, setRefreshTokenFingerprint, setRefreshTokenIndex, addToUser, addExpiration);
		return true;
	}

	public Task<RedisValue> GetAsync(string sessionId)
	{
		return _database.StringGetAsync(RedisKeys.Sessions.ById(sessionId));
	}

	public Task<RedisValue[]> GetManyAsync(RedisValue[] sessionIds)
	{
		var keys = sessionIds.Select(sessionId => RedisKeys.Sessions.ById(sessionId.ToString())).ToArray();
		return _database.StringGetAsync(keys);
	}

	public Task<RedisValue[]> ListIdsByUserAsync(int userId)
	{
		return _database.SortedSetRangeByRankAsync(RedisKeys.Sessions.ByUser(userId), 0, -1, Order.Descending);
	}

	public Task RemoveFromUserIndexAsync(int userId, RedisValue[] sessionIds)
	{
		return _database.SortedSetRemoveAsync(RedisKeys.Sessions.ByUser(userId), sessionIds);
	}

	public Task<TimeSpan?> GetTtlAsync(string sessionId)
	{
		return _database.KeyTimeToLiveAsync(RedisKeys.Sessions.ById(sessionId));
	}

	public async Task TerminateAsync(UserSessionDto session)
	{
		await _database.ScriptEvaluateAsync(
			SessionRedisScripts.TerminateSession,
			[
				RedisKeys.Sessions.ById(session.Id),
				RedisKeys.Sessions.RefreshTokenFingerprint(session.Id),
				RedisKeys.Sessions.ByUser(session.UserId),
				RedisKeys.Sessions.ExpirationIndex,
			],
			[
				session.Id,
				RedisKeys.Sessions.ExpirationMember(session.UserId, session.Id),
				RedisKeys.Sessions.RefreshTokenFingerprintIndexPrefix,
			]
		);
	}
}
