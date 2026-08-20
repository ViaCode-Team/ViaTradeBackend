using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;
using ViaTrade.Application.Users.Models;
using ViaTrade.Infrastructure.Redis.Keys;
using ViaTrade.Infrastructure.Redis.Scripts;
using ViaTrade.Infrastructure.Redis.Serialization;

namespace ViaTrade.Infrastructure.Redis.Utils;

internal sealed class RefreshTokenRedisHelper(IDatabase database)
{
	private readonly IDatabase _database = database;

	public Task<RedisValue> FindSessionIdAsync(string refreshTokenFingerprint)
	{
		return _database.StringGetAsync(RedisKeys.Sessions.RefreshTokenIndex(refreshTokenFingerprint));
	}

	public Task<RedisValue> FindUsedSessionIdAsync(string refreshTokenFingerprint)
	{
		return _database.StringGetAsync(RedisKeys.Sessions.UsedRefreshToken(refreshTokenFingerprint));
	}

	public async Task<bool> TryRotateAsync(
		UserSessionDto session,
		string refreshTokenFingerprint,
		string newRefreshTokenFingerprint,
		TimeSpan sessionTtl,
		TimeSpan usedRefreshTokenTtl
	)
	{
		var result = await _database.ScriptEvaluateAsync(
			SessionRedisScripts.RotateRefresh,
			[
				RedisKeys.Sessions.ById(session.Id),
				RedisKeys.Sessions.RefreshTokenFingerprint(session.Id),
				RedisKeys.Sessions.RefreshTokenIndex(refreshTokenFingerprint),
				RedisKeys.Sessions.RefreshTokenIndex(newRefreshTokenFingerprint),
				RedisKeys.Sessions.UsedRefreshToken(refreshTokenFingerprint),
				RedisKeys.Sessions.ByUser(session.UserId),
				RedisKeys.Sessions.ExpirationIndex,
			],
			[
				refreshTokenFingerprint,
				session.Id,
				JsonSerializer.Serialize(session, RedisJsonSerializerContext.Default.UserSessionDto),
				ToMilliseconds(sessionTtl),
				newRefreshTokenFingerprint,
				ToMilliseconds(usedRefreshTokenTtl),
				session.CreatedAt.Ticks,
				session.ExpiresAt.Ticks,
				RedisKeys.Sessions.ExpirationMember(session.UserId, session.Id),
			]
		);

		return (int)result == 1;
	}

	public async Task<bool> TryMigrateLegacyAsync(
		UserSessionDto session,
		string refreshToken,
		string refreshTokenFingerprint,
		TimeSpan sessionTtl
	)
	{
		var transaction = _database.CreateTransaction();
		transaction.AddCondition(Condition.KeyExists(RedisKeys.Sessions.ById(session.Id)));
		transaction.AddCondition(
			Condition.StringEqual(RedisKeys.Sessions.RefreshTokenFingerprint(session.Id), refreshToken)
		);
		transaction.AddCondition(Condition.StringEqual(RedisKeys.Sessions.RefreshTokenIndex(refreshToken), session.Id));
		transaction.AddCondition(Condition.KeyNotExists(RedisKeys.Sessions.RefreshTokenIndex(refreshTokenFingerprint)));

		var setRefreshTokenFingerprint = transaction.StringSetAsync(
			RedisKeys.Sessions.RefreshTokenFingerprint(session.Id),
			refreshTokenFingerprint,
			sessionTtl
		);
		var removeLegacyRefreshTokenIndex = transaction.KeyDeleteAsync(
			RedisKeys.Sessions.RefreshTokenIndex(refreshToken)
		);
		var setRefreshTokenIndex = transaction.StringSetAsync(
			RedisKeys.Sessions.RefreshTokenIndex(refreshTokenFingerprint),
			session.Id,
			sessionTtl
		);

		if (!await transaction.ExecuteAsync())
			return false;

		await Task.WhenAll(setRefreshTokenFingerprint, removeLegacyRefreshTokenIndex, setRefreshTokenIndex);
		return true;
	}

	public static string GetFingerprint(string refreshToken)
	{
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
	}

	private static long ToMilliseconds(TimeSpan ttl)
	{
		var milliseconds = (long)Math.Ceiling(ttl.TotalMilliseconds);
		if (milliseconds < 1)
			throw new ArgumentOutOfRangeException(nameof(ttl), "Redis key TTL must be positive.");

		return milliseconds;
	}
}
