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
			CreateCookieOptions(DateTimeOffset.UtcNow.AddHours(1))
		);

		response.Cookies.Append(
			_authCookieOptions.RefreshTokenCookie,
			tokens.RefreshToken,
			CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(_authCookieOptions.RefreshTokenExpiryDays))
		);
	}

	public void DeleteAuthCookies(HttpResponse response)
	{
		response.Cookies.Delete(_authCookieOptions.AccessTokenCookie);
		response.Cookies.Delete(_authCookieOptions.RefreshTokenCookie);
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
