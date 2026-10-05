using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Configuration.Options;
using ViaTrade.Infrastructure.Utils;
using Xunit;

namespace ViaTrade.Tests;

public sealed class AuthTokenFactoryTests
{
	[Theory]
	[InlineData(0, 7)]
	[InlineData(29, 1)]
	[InlineData(31, -1)]
	public void SessionExpiryUsesTheEarlierIdleOrAbsoluteLimit(int sessionAgeDays, int remainingDays)
	{
		var factory = CreateFactory();
		var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		Assert.Equal(now.AddDays(remainingDays), factory.CalculateExpiresAt(now.AddDays(-sessionAgeDays), now));
	}

	[Theory]
	[InlineData(2, 2)]
	[InlineData(60, 10)]
	public void AccessTokenCannotOutliveTheSession(int sessionMinutes, int accessMinutes)
	{
		var factory = CreateFactory();
		var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var session = new SessionData
		{
			Id = "session",
			UserId = 7,
			UserAgent = "browser",
			CreatedAt = now,
			LastSeen = now,
			ExpiresAt = now.AddMinutes(sessionMinutes),
		};

		var tokens = factory.CreateAuthTokens(new TokenUser(7, "test"), session, "refresh", now);
		Assert.Equal(new DateTimeOffset(now.AddMinutes(accessMinutes)), tokens.AccessTokenExpiresAt);
		Assert.Equal(new DateTimeOffset(session.ExpiresAt), tokens.RefreshTokenExpiresAt);
		Assert.Equal("refresh", tokens.RefreshToken);
		var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
		Assert.Equal(now.AddMinutes(accessMinutes), jwt.ValidTo);
		Assert.Equal("7", jwt.Subject);
		Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Jti && claim.Value == "session");
	}

	private static AuthTokenFactory CreateFactory()
	{
		var jwtOptions = Options.Create(
			new JwtSettings
			{
				Secret = "test-key-for-token-factory-tests-only-32-bytes",
				Issuer = "tests",
				Audience = "tests",
				AccessTokenMinutes = 10,
			}
		);
		var cookieOptions = Options.Create(
			new AuthCookieSettings { RefreshTokenExpiryDays = 7, AbsoluteSessionLifetimeDays = 30 }
		);
		return new AuthTokenFactory(new JwtHelper(jwtOptions), jwtOptions, cookieOptions);
	}
}
