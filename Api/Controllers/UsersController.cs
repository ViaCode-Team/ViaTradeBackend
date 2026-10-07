using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Security.Authentication.Cookies;
using ViaTrade.Api.Swagger.Attributes;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Auth.Register;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Users.CreateTelegramLink;
using ViaTrade.Application.Users.GetCurrent;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class UsersController(IAuthCookieService authCookieService, ILogger<UsersController> logger) : ControllerBase
{
	[HttpPost]
	[AllowAnonymous]
	[SetsAuthCookies]
	public async Task<NoContent> Register(
		[FromBody] RegisterCommand command,
		[FromServices] ICommandHandler<RegisterCommand> registerHandler,
		[FromServices] ICommandHandler<LoginCommand, AuthTokensResult> loginHandler,
		CancellationToken ct
	)
	{
		await registerHandler.HandleAsync(command, ct);

		var userAgent = Request.Headers.UserAgent.ToString();

		var loginCommand = new LoginCommand(command.Login, command.Password, userAgent);
		var tokens = await loginHandler.HandleAsync(loginCommand, ct);

		authCookieService.SetAuthCookies(Response, tokens);
		return TypedResults.NoContent();
	}

	[HttpGet("me")]
	public async Task<Results<Ok<CurrentUserResult>, NotFound>> GetMe(
		[FromServices] IQueryHandler<GetCurrentUserQuery, CurrentUserResult> handler,
		CancellationToken ct
	)
	{
		logger.LogInformation("Getting current user information");

		var user = await handler.HandleAsync(new GetCurrentUserQuery(), ct);

		return TypedResults.Ok(user);
	}

	[HttpPost("me/telegramLinkToken")]
	public async Task<Ok<TelegramLinkResult>> GenerateTelegramToken(
		[FromServices] ICommandHandler<CreateTelegramLinkCommand, TelegramLinkResult> handler,
		CancellationToken ct
	)
	{
		logger.LogInformation("Generating Telegram token for user");

		var token = await handler.HandleAsync(new CreateTelegramLinkCommand(), ct);

		return TypedResults.Ok(token);
	}
}
