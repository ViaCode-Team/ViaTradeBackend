using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Security.Authentication.Cookies;
using ViaTrade.Api.Swagger.Attributes;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.GetSessionsPage;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Auth.LogoutAll;
using ViaTrade.Application.Auth.LogoutSession;
using ViaTrade.Application.Auth.RefreshTokens;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class SessionsController(IAuthCookieService authCookieService, IOptions<AuthCookieSettings> authOptions)
	: ControllerBase
{
	private readonly AuthCookieSettings _authCookieOptions = authOptions.Value;

	[HttpPost]
	[AllowAnonymous]
	[SetsAuthCookies]
	public async Task<NoContent> Login(
		[FromBody, IgnoreProperties(nameof(LoginCommand.UserAgent))] LoginCommand command,
		[FromServices] ICommandHandler<LoginCommand, AuthTokensResult> handler,
		CancellationToken ct
	)
	{
		var userAgent = Request.Headers.UserAgent.ToString();

		command = command with { UserAgent = userAgent };
		var tokens = await handler.HandleAsync(command, ct);

		authCookieService.SetAuthCookies(Response, tokens);
		return TypedResults.NoContent();
	}

	[HttpPost("current/tokens")]
	[AllowAnonymous]
	[SetsAuthCookies]
	public async Task<NoContent> CreateCurrentSessionTokens(
		[FromServices] ICommandHandler<RefreshTokensCommand, AuthTokensResult> handler,
		CancellationToken ct
	)
	{
		var hasRefreshToken = Request.Cookies.TryGetValue(_authCookieOptions.RefreshTokenCookie, out var refreshToken);

		if (!hasRefreshToken || string.IsNullOrWhiteSpace(refreshToken))
			throw new UnauthorizedAccessException();

		var tokens = await handler.HandleAsync(new RefreshTokensCommand(refreshToken), ct);

		authCookieService.SetAuthCookies(Response, tokens);
		return TypedResults.NoContent();
	}

	[HttpDelete("current")]
	public async Task<NoContent> DeleteCurrentSession(
		[FromServices] ICommandHandler<LogoutSessionCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(new LogoutSessionCommand(), ct);

		authCookieService.DeleteAuthCookies(Response);
		return TypedResults.NoContent();
	}

	[HttpDelete]
	public async Task<NoContent> DeleteSessions(
		[FromServices] ICommandHandler<LogoutAllCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(new LogoutAllCommand(), ct);

		authCookieService.DeleteAuthCookies(Response);
		return TypedResults.NoContent();
	}

	[HttpGet]
	public async Task<Ok<PageResult<SessionResult>>> GetSessions(
		[FromQuery] GetSessionsPageQuery query,
		[FromServices] IQueryHandler<GetSessionsPageQuery, PageResult<SessionResult>> handler,
		CancellationToken ct
	)
	{
		var userSessions = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(userSessions);
	}
}
