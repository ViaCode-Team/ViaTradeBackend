using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ViaTradeBackend.Contracts.Auth;
using ViaTradeBackend.Contracts.Users;
using ViaTradeBackend.Cookies;
using ViaTradeBackend.Mappings;
using ViaTradeBackend.Swagger.Attributes;

namespace ViaTradeBackend.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class SessionsController(
	IAuthCommandService authCommandService,
	IAuthQueryService authQueryService,
	IJwtHelper jwtHelper,
	IAuthCookieService authCookieService,
	IOptions<AuthCookieOptions> authOptions
) : ControllerBase
{
	private readonly AuthCookieOptions _authCookieOptions = authOptions.Value;

	[HttpPost]
	[AllowAnonymous]
	[SetsAuthCookies]
	public async Task<NoContent> Login([FromBody, Required] LoginRequest request, CancellationToken ct)
	{
		var userAgent = Request.Headers.UserAgent.ToString();
		var tokens = await authCommandService.LoginAsync(request.Login, request.Password, userAgent, ct);

		authCookieService.SetAuthCookies(Response, tokens);
		return TypedResults.NoContent();
	}

	[HttpPost("current/refresh")]
	[AllowAnonymous]
	public async Task<NoContent> RefreshCurrentSession(CancellationToken ct)
	{
		var hasRefreshToken = Request.Cookies.TryGetValue(_authCookieOptions.RefreshTokenCookie, out var refreshToken);

		if (!hasRefreshToken || string.IsNullOrWhiteSpace(refreshToken))
			throw new UnauthorizedAccessException();

		var tokens = await authCommandService.RefreshTokenAsync(refreshToken, ct);

		authCookieService.SetAuthCookies(Response, tokens);
		return TypedResults.NoContent();
	}

	[HttpDelete("current")]
	public async Task<NoContent> DeleteCurrentSession(CancellationToken ct)
	{
		var sessionId = jwtHelper.GetSessionId(User);
		await authCommandService.LogoutSessionAsync(sessionId, ct);

		authCookieService.DeleteAuthCookies(Response);
		return TypedResults.NoContent();
	}

	[HttpDelete]
	public async Task<NoContent> DeleteSessions(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await authCommandService.LogoutAllAsync(userId, ct);

		authCookieService.DeleteAuthCookies(Response);
		return TypedResults.NoContent();
	}

	[HttpGet]
	public async Task<Ok<PageResult<UserSessionResponse>>> GetSessions(
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var currentSessionId = jwtHelper.GetSessionId(User);
		var userSessions = await authQueryService.GetSessionsPageAsync(userId, pageOptions, ct);

		return TypedResults.Ok(userSessions.Map(session => ApiMapper.ToResponse(session, currentSessionId)));
	}
}
