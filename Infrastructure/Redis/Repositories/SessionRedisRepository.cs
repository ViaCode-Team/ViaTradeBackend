using System.Security.Cryptography;
using System.Text;
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
	private const int TerminateSessionMaxAttempts = 3;

	private readonly IDatabase _redis = redis.GetDatabase();

	private static string SessionKey(string sessionId) => $"session:{sessionId}";

	private static string UserSessionsKey(int userId) => $"user:sessions:{userId}";

	private static RedisKey SessionExpirationsKey => "sessions:expires";

	private static string ExpirationMember(UserSessionDto session) => $"{session.UserId}:{session.Id}";

	private static string TokenKey(string sessionId) => $"refresh:{sessionId}";

	private static string IndexKey(string refreshToken) => $"refresh:idx:{refreshToken}";

	private static string UsedTokenKey(string refreshTokenFingerprint) => $"refresh:used:{refreshTokenFingerprint}";

	public async Task CreateSessionAsync(UserSessionDto session, string refreshToken, TimeSpan ttl)
	{
		var json = JsonSerializer.Serialize(session);
		var transaction = _redis.CreateTransaction();
		var setSession = transaction.StringSetAsync(SessionKey(session.Id), json, ttl);
		var setRefreshToken = transaction.StringSetAsync(TokenKey(session.Id), refreshToken, ttl);
		var setRefreshTokenIndex = transaction.StringSetAsync(IndexKey(refreshToken), session.Id, ttl);
		var addToUser = transaction.SortedSetAddAsync(UserSessionsKey(session.UserId), session.Id, session.CreatedAt.Ticks);
		var addExpiration = transaction.SortedSetAddAsync(
			SessionExpirationsKey,
			ExpirationMember(session),
			session.ExpiresAt.Ticks
		);

		if (!await transaction.ExecuteAsync())
			throw new Exception("Failed to create session in Redis.");

		await Task.WhenAll(setSession, setRefreshToken, setRefreshTokenIndex, addToUser, addExpiration);
	}

	public async Task<UserSessionDto?> FindByIdAsync(string sessionId)
	{
		var value = await _redis.StringGetAsync(SessionKey(sessionId));
		if (value.IsNullOrEmpty)
			return null;

		return JsonSerializer.Deserialize<UserSessionDto>(value.ToString());
	}

	public async Task<UserSessionDto?> FindByRefreshTokenAsync(string refreshToken)
	{
		var sessionId = await _redis.StringGetAsync(IndexKey(refreshToken));
		if (sessionId.IsNullOrEmpty)
			return null;

		return await FindByIdAsync(sessionId.ToString());
	}

	public async Task<bool> TryTerminateSessionByUsedRefreshTokenAsync(string refreshToken)
	{
		var refreshTokenFingerprint = GetRefreshTokenFingerprint(refreshToken);
		var sessionId = await _redis.StringGetAsync(UsedTokenKey(refreshTokenFingerprint));
		if (sessionId.IsNullOrEmpty)
			return false;

		await TerminateSessionAsync(sessionId.ToString());
		return true;
	}

	public async Task<bool> TryRotateRefreshAsync(
		UserSessionDto session,
		string refreshToken,
		string newRefreshToken,
		TimeSpan sessionTtl,
		TimeSpan usedRefreshTokenTtl
	)
	{
		var json = JsonSerializer.Serialize(session);
		var refreshTokenFingerprint = GetRefreshTokenFingerprint(refreshToken);

		var transaction = _redis.CreateTransaction();

		transaction.AddCondition(Condition.KeyExists(SessionKey(session.Id)));
		transaction.AddCondition(Condition.StringEqual(TokenKey(session.Id), refreshToken));
		transaction.AddCondition(Condition.StringEqual(IndexKey(refreshToken), session.Id));
		transaction.AddCondition(Condition.KeyNotExists(IndexKey(newRefreshToken)));
		transaction.AddCondition(Condition.KeyNotExists(UsedTokenKey(refreshTokenFingerprint)));

		var setSession = transaction.StringSetAsync(SessionKey(session.Id), json, sessionTtl);
		var setRefreshToken = transaction.StringSetAsync(TokenKey(session.Id), newRefreshToken, sessionTtl);
		var removeRefreshTokenIndex = transaction.KeyDeleteAsync(IndexKey(refreshToken));
		var setRefreshTokenIndex = transaction.StringSetAsync(
			IndexKey(newRefreshToken),
			session.Id,
			sessionTtl
		);
		var setUsedRefreshToken = transaction.StringSetAsync(
			UsedTokenKey(refreshTokenFingerprint),
			session.Id,
			usedRefreshTokenTtl
		);
		var updateUserSession = transaction.SortedSetAddAsync(
			UserSessionsKey(session.UserId),
			session.Id,
			session.CreatedAt.Ticks
		);
		var updateExpiration = transaction.SortedSetAddAsync(
			SessionExpirationsKey,
			ExpirationMember(session),
			session.ExpiresAt.Ticks
		);

		if (!await transaction.ExecuteAsync())
			return false;

		await Task.WhenAll(
			setSession,
			setRefreshToken,
			removeRefreshTokenIndex,
			setRefreshTokenIndex,
			setUsedRefreshToken,
			updateUserSession,
			updateExpiration
		);

		return true;
	}

	public async Task TerminateSessionAsync(string sessionId)
	{
		for (int attempt = 0; attempt < TerminateSessionMaxAttempts; attempt++)
		{
			var session = await FindByIdAsync(sessionId);
			if (session == null)
				return;

			var refreshToken = await _redis.StringGetAsync(TokenKey(session.Id));

			var transaction = _redis.CreateTransaction();

			transaction.AddCondition(Condition.KeyExists(SessionKey(session.Id)));

			if (refreshToken.IsNullOrEmpty)
				transaction.AddCondition(Condition.KeyNotExists(TokenKey(session.Id)));
			else
				transaction.AddCondition(Condition.StringEqual(TokenKey(session.Id), refreshToken));

			var removeSession = transaction.KeyDeleteAsync(SessionKey(session.Id));
			var removeRefreshToken = transaction.KeyDeleteAsync(TokenKey(session.Id));
			var removeFromUser = transaction.SortedSetRemoveAsync(UserSessionsKey(session.UserId), session.Id);
			var removeExpiration = transaction.SortedSetRemoveAsync(SessionExpirationsKey, ExpirationMember(session));
			Task<bool>? removeRefreshTokenIndex = null;

			if (!refreshToken.IsNullOrEmpty)
				removeRefreshTokenIndex = transaction.KeyDeleteAsync(IndexKey(refreshToken.ToString()));

			if (!await transaction.ExecuteAsync())
				continue;

			if (removeRefreshTokenIndex == null)
			{
				await Task.WhenAll(removeSession, removeRefreshToken, removeFromUser, removeExpiration);
				return;
			}

			await Task.WhenAll(
				removeSession,
				removeRefreshToken,
				removeFromUser,
				removeExpiration,
				removeRefreshTokenIndex
			);
			return;
		}

		throw new InvalidOperationException("Failed to terminate session because it was modified concurrently.");
	}

	public async Task<IReadOnlyList<UserSessionDto>> ListByUserAsync(int userId)
	{
		var sessionIds = await _redis.SortedSetRangeByRankAsync(UserSessionsKey(userId), 0, -1);
		if (sessionIds.Length == 0)
			return [];

		var keys = sessionIds
			.Where(id => !id.IsNullOrEmpty)
			.Select(id => (RedisKey)SessionKey(id.ToString()))
			.ToArray();
		var values = await _redis.StringGetAsync(keys);

		List<UserSessionDto> result = [];

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
		var totalCount = await _redis.SortedSetLengthAsync(UserSessionsKey(userId));

		int start = (pageOptions.Page - 1) * pageOptions.PageSize;
		int stop = start + pageOptions.PageSize - 1;

		var sessionIds = await _redis.SortedSetRangeByRankAsync(UserSessionsKey(userId), start, stop, Order.Descending);

		List<UserSessionDto> result = [];

		if (sessionIds.Length > 0)
		{
			var keys = sessionIds
				.Where(id => !id.IsNullOrEmpty)
				.Select(id => (RedisKey)SessionKey(id.ToString()))
				.ToArray();
			var values = await _redis.StringGetAsync(keys);

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
		var expiredSessions = await _redis.SortedSetRangeByScoreWithScoresAsync(
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
				await _redis.SortedSetRemoveAsync(SessionExpirationsKey, expiredSession.Element);
				totalDeleted++;
				continue;
			}

			try
			{
				var transaction = _redis.CreateTransaction();
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

	private static string GetRefreshTokenFingerprint(string refreshToken)
	{
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
	}

}
