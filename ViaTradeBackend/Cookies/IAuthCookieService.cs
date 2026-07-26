using Application.Auth.Models;

namespace ViaTradeBackend.Cookies;

public interface IAuthCookieService
{
	void SetAuthCookies(HttpResponse response, AuthTokens tokens);
	void DeleteAuthCookies(HttpResponse response);
}
