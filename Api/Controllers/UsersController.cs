using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Api.Cookies;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Swagger.Attributes;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Auth.Register;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Users.CreateTelegramLink;
using ViaTrade.Application.Users.GetCurrent;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class UsersController(
	IJwtHelper jwtHelper,
	IAuthCookieService authCookieService,
	ILogger<UsersController> logger
) : ControllerBase
{
	[HttpPost]
	[AllowAnonymous]
	[SetsAuthCookies]
	public async Task<NoContent> Register(
		[FromBody, IgnoreProperties(nameof(RegisterCommand.UserAgent))] RegisterCommand command,
		[FromServices] ICommandHandler<RegisterCommand, AuthTokensResult> handler,
		CancellationToken ct
	)
	{
		var userAgent = Request.Headers.UserAgent.ToString();

		command = command with { UserAgent = userAgent };
		var tokens = await handler.HandleAsync(command, ct);

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

		var userId = jwtHelper.GetUserIdFromClaims(User);

		var user = await handler.HandleAsync(new GetCurrentUserQuery(userId), ct);

		return TypedResults.Ok(user);
	}

	[HttpPost("me/telegramLinkToken")]
	public async Task<Ok<TelegramLinkResult>> GenerateTelegramToken(
		[FromServices] ICommandHandler<CreateTelegramLinkCommand, TelegramLinkResult> handler,
		CancellationToken ct
	)
	{
		logger.LogInformation("Generating Telegram token for user");

		var userId = jwtHelper.GetUserIdFromClaims(User);

		var token = await handler.HandleAsync(new CreateTelegramLinkCommand(userId), ct);

		return TypedResults.Ok(token);
	}
}
