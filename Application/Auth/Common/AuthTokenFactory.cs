using Microsoft.Extensions.Options;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Application.Auth.Common;

public sealed class AuthTokenFactory(
	IJwtHelper jwtHelper,
	IOptions<JwtSettings> jwtOptions,
	IOptions<AuthCookieSettings> authCookieOptions
)
{
	private readonly TimeSpan _accessTokenLifetime = TimeSpan.FromMinutes(jwtOptions.Value.AccessTokenMinutes);
	private readonly TimeSpan _idleSessionLifetime = TimeSpan.FromDays(authCookieOptions.Value.RefreshTokenExpiryDays);
	private readonly TimeSpan _absoluteSessionLifetime = TimeSpan.FromDays(
		authCookieOptions.Value.AbsoluteSessionLifetimeDays
	);

	public TimeSpan AbsoluteSessionLifetime => _absoluteSessionLifetime;

	public DateTime CalculateExpiresAt(DateTime createdAt, DateTime now)
	{
		var idleExpiresAt = now.Add(_idleSessionLifetime);
		var absoluteExpiresAt = createdAt.Add(_absoluteSessionLifetime);
		if (idleExpiresAt < absoluteExpiresAt)
			return idleExpiresAt;

		return absoluteExpiresAt;
	}

	public AuthTokensResult CreateAuthTokens(TokenUser user, SessionData session, string refreshToken, DateTime now)
	{
		var configuredAccessTokenExpiresAt = now.Add(_accessTokenLifetime);
		var accessTokenExpiresAt = configuredAccessTokenExpiresAt;
		if (session.ExpiresAt < configuredAccessTokenExpiresAt)
			accessTokenExpiresAt = session.ExpiresAt;

		var accessToken = jwtHelper.GenerateAccessToken(user, session.Id, accessTokenExpiresAt);

		return new AuthTokensResult
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			AccessTokenExpiresAt = new DateTimeOffset(accessTokenExpiresAt),
			RefreshTokenExpiresAt = new DateTimeOffset(session.ExpiresAt),
		};
	}
}
