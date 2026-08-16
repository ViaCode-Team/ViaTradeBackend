using Microsoft.Extensions.Options;
using ViaTrade.Application.Auth.Models;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.Cookies;

public sealed class AuthCookieService(IOptions<AuthCookieSettings> authOptions) : IAuthCookieService
{
	private readonly AuthCookieSettings _authCookieOptions = authOptions.Value;

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
