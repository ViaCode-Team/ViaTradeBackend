using Application.Auth.Models;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ViaTradeBackend.Cookies;

public sealed class AuthCookieService(IOptions<AuthCookieOptions> authOptions) : IAuthCookieService
{
	private readonly AuthCookieOptions _authCookieOptions = authOptions.Value;

	public void SetAuthCookies(HttpResponse response, AuthTokens tokens)
	{
		response.Cookies.Append(
			_authCookieOptions.AccessTokenCookie,
			tokens.AccessToken,
			CreateCookieOptions(tokens.AccessTokenExpiresAt)
		);

		response.Cookies.Append(
			_authCookieOptions.RefreshTokenCookie,
			tokens.RefreshToken,
			CreateCookieOptions(tokens.RefreshTokenExpiresAt)
		);
	}

	public void DeleteAuthCookies(HttpResponse response)
	{
		var expiredCookieOptions = CreateCookieOptions(DateTimeOffset.UnixEpoch);

		response.Cookies.Delete(_authCookieOptions.AccessTokenCookie, expiredCookieOptions);
		response.Cookies.Delete(_authCookieOptions.RefreshTokenCookie, expiredCookieOptions);
	}

	private static CookieOptions CreateCookieOptions(DateTimeOffset expires)
	{
		return new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Strict,
			Expires = expires,
			Path = "/",
		};
	}
}
