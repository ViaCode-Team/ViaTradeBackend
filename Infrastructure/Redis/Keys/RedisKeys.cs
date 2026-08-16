using StackExchange.Redis;

namespace ViaTrade.Infrastructure.Redis.Keys;

internal static class RedisKeys
{
	public static class Cache
	{
		public static RedisKeyBuilder Users { get; } = RedisKeyBuilder.Create("User");

		public static RedisKeyBuilder TelegramTokens { get; } = RedisKeyBuilder.Create("TgToken");
	}

	public static class Sessions
	{
		private const char ExpirationMemberSeparator = ':';
		private static readonly RedisKeyBuilder Session = RedisKeyBuilder.Create("session");
		private static readonly RedisKeyBuilder UserSessions = RedisKeyBuilder.Create("user").Append("sessions");
		private static readonly RedisKeyBuilder RefreshToken = RedisKeyBuilder.Create("refresh");
		private static readonly RedisKeyBuilder RefreshTokenIndexes = RefreshToken.Append("idx");
		private static readonly RedisKeyBuilder UsedRefreshTokens = RefreshToken.Append("used");
		private static readonly RedisKeyBuilder SessionIndexes = RedisKeyBuilder.Create("sessions");

		public static string RefreshTokenFingerprintIndexPrefix { get; } = RefreshTokenIndexes.Prefix;

		public static RedisKey ExpirationIndex { get; } = SessionIndexes.Build("expires");

		public static RedisKey ById(string sessionId) => Session.Build(sessionId);

		public static RedisKey ByUser(int userId) => UserSessions.Build(userId);

		public static RedisKey RefreshTokenFingerprint(string sessionId) => RefreshToken.Build(sessionId);

		public static RedisKey RefreshTokenIndex(string refreshTokenFingerprint) =>
			RefreshTokenIndexes.Build(refreshTokenFingerprint);

		public static RedisKey UsedRefreshToken(string refreshTokenFingerprint) =>
			UsedRefreshTokens.Build(refreshTokenFingerprint);

		public static RedisValue ExpirationMember(int userId, string sessionId) =>
			$"{userId}{ExpirationMemberSeparator}{sessionId}";

		public static bool TryParseExpirationMember(RedisValue value, out int userId, out string sessionId)
		{
			var expirationMember = value.ToString();
			var separatorIndex = expirationMember.IndexOf(ExpirationMemberSeparator);

			if (
				separatorIndex > 0
				&& separatorIndex < expirationMember.Length - 1
				&& int.TryParse(expirationMember.AsSpan(0, separatorIndex), out userId)
				&& !expirationMember.AsSpan(separatorIndex + 1).Trim().IsEmpty
			)
			{
				sessionId = expirationMember[(separatorIndex + 1)..];
				return true;
			}

			userId = default;
			sessionId = string.Empty;
			return false;
		}
	}
}
