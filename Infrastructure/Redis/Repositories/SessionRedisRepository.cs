using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ViaTrade.Application.Auth.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Users.Models;
using ViaTrade.Infrastructure.Redis.Utils;

namespace ViaTrade.Infrastructure.Redis.Repositories;

public class SessionRedisRepository(
	IConnectionMultiplexer connectionMultiplexer,
	ILogger<SessionRedisRepository> logger
) : ISessionRepository
{
	private readonly SessionRedisStorageHelper _sessionStorageHelper = new(connectionMultiplexer.GetDatabase());
	private readonly RefreshTokenRedisHelper _refreshTokenHelper = new(connectionMultiplexer.GetDatabase());
	private readonly SessionRedisCleanupHelper _sessionCleanupHelper = new(connectionMultiplexer.GetDatabase());

	public async Task CreateSessionAsync(UserSessionDto session, string refreshToken, TimeSpan ttl)
	{
		var refreshTokenFingerprint = RefreshTokenRedisHelper.GetFingerprint(refreshToken);
		var sessionJson = JsonSerializer.Serialize(session);
		var wasCreated = await _sessionStorageHelper.TryCreateAsync(session, sessionJson, refreshTokenFingerprint, ttl);
		if (wasCreated)
			return;

		logger.LogError("Redis transaction failed while creating session: SessionId={SessionId}", session.Id);
		throw new InvalidOperationException("Failed to create session in Redis.");
	}

	public async Task<UserSessionDto?> FindByIdAsync(string sessionId)
	{
		var value = await _sessionStorageHelper.GetAsync(sessionId);
		if (value.IsNullOrEmpty)
			return null;

		return DeserializeSession(value, sessionId);
	}

	public async Task<UserSessionDto?> FindByRefreshTokenAsync(string refreshToken)
	{
		var refreshTokenFingerprint = RefreshTokenRedisHelper.GetFingerprint(refreshToken);
		var sessionId = await _refreshTokenHelper.FindSessionIdAsync(refreshTokenFingerprint);
		if (!sessionId.IsNullOrEmpty)
			return await FindByIdAsync(sessionId.ToString());

		return await FindAndMigrateLegacySessionAsync(refreshToken, refreshTokenFingerprint);
	}

	public async Task<bool> TryTerminateSessionByUsedRefreshTokenAsync(string refreshToken)
	{
		var refreshTokenFingerprint = RefreshTokenRedisHelper.GetFingerprint(refreshToken);
		var sessionId = await _refreshTokenHelper.FindUsedSessionIdAsync(refreshTokenFingerprint);
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
		var refreshTokenFingerprint = RefreshTokenRedisHelper.GetFingerprint(refreshToken);
		var newRefreshTokenFingerprint = RefreshTokenRedisHelper.GetFingerprint(newRefreshToken);

		return await _refreshTokenHelper.TryRotateAsync(
			session,
			refreshTokenFingerprint,
			newRefreshTokenFingerprint,
			sessionTtl,
			usedRefreshTokenTtl
		);
	}

	public async Task TerminateSessionAsync(string sessionId)
	{
		var session = await FindByIdAsync(sessionId);
		if (session == null)
			return;

		await _sessionStorageHelper.TerminateAsync(session);
	}

	public async Task<IReadOnlyList<UserSessionDto>> ListByUserAsync(int userId)
	{
		var sessionIds = await _sessionStorageHelper.ListIdsByUserAsync(userId);

		return await LoadExistingSessionsAsync(userId, sessionIds);
	}

	public async Task<PageResult<UserSessionDto>> GetPageByUserAsync(int userId, PageOptions pageOptions)
	{
		var sessions = await ListByUserAsync(userId);
		var start = (pageOptions.Page - 1) * pageOptions.PageSize;
		var pageItems = sessions.Skip(start).Take(pageOptions.PageSize).ToList();

		return new PageResult<UserSessionDto>(pageItems, sessions.Count, pageOptions.Page, pageOptions.PageSize);
	}

	public Task<int> CleanupExpiredSessionsAsync(DateTime utcNow) =>
		_sessionCleanupHelper.CleanupExpiredSessionsAsync(utcNow);

	private async Task<List<UserSessionDto>> LoadExistingSessionsAsync(int userId, RedisValue[] sessionIds)
	{
		if (sessionIds.Length == 0)
			return [];

		var values = await _sessionStorageHelper.GetManyAsync(sessionIds);
		List<UserSessionDto> sessions = [];
		List<RedisValue> staleSessionIds = [];

		for (var index = 0; index < sessionIds.Length; index++)
		{
			var sessionId = sessionIds[index].ToString();
			var value = values[index];
			if (value.IsNullOrEmpty)
			{
				staleSessionIds.Add(sessionIds[index]);
				continue;
			}

			var session = DeserializeSession(value, sessionId);
			if (session.UserId != userId)
				throw new InvalidOperationException("Redis session index contains a session for a different user.");

			sessions.Add(session);
		}

		if (staleSessionIds.Count > 0)
			await _sessionStorageHelper.RemoveFromUserIndexAsync(userId, staleSessionIds.ToArray());

		return sessions;
	}

	private async Task<UserSessionDto?> FindAndMigrateLegacySessionAsync(
		string refreshToken,
		string refreshTokenFingerprint
	)
	{
		var sessionId = await _refreshTokenHelper.FindSessionIdAsync(refreshToken);
		if (sessionId.IsNullOrEmpty)
			return null;

		var session = await FindByIdAsync(sessionId.ToString());
		if (session == null)
			return null;

		var sessionTtl = await _sessionStorageHelper.GetTtlAsync(session.Id);
		if (sessionTtl == null || sessionTtl <= TimeSpan.Zero)
			return null;

		var wasMigrated = await _refreshTokenHelper.TryMigrateLegacyAsync(
			session,
			refreshToken,
			refreshTokenFingerprint,
			sessionTtl.Value
		);
		if (!wasMigrated)
			return null;

		return session;
	}

	private UserSessionDto DeserializeSession(RedisValue value, string expectedSessionId)
	{
		try
		{
			var session = JsonSerializer.Deserialize<UserSessionDto>(value.ToString());
			if (session == null || session.Id != expectedSessionId)
				throw new InvalidOperationException("Redis session data is invalid.");

			return session;
		}
		catch (JsonException exception)
		{
			logger.LogError(
				exception,
				"Redis session data cannot be deserialized: SessionId={SessionId}",
				expectedSessionId
			);
			throw new InvalidOperationException("Redis session data is invalid.", exception);
		}
	}
}
