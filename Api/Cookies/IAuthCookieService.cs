using ViaTrade.Application.Auth.Common;

namespace ViaTrade.Api.Cookies;

public interface IAuthCookieService
{
	void SetAuthCookies(HttpResponse response, AuthTokensResult tokens);
	void DeleteAuthCookies(HttpResponse response);
}
