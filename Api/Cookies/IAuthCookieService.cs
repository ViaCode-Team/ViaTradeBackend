using ViaTrade.Application.Auth.Models;

namespace ViaTrade.Api.Cookies;

public interface IAuthCookieService
{
	void SetAuthCookies(HttpResponse response, AuthTokens tokens);
	void DeleteAuthCookies(HttpResponse response);
}
