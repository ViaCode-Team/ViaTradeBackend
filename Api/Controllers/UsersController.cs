using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Security.Authentication.Cookies;
using ViaTrade.Api.Swagger.Attributes;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Auth.Register;
using ViaTrade.Application.Users.CreateTelegramLink;
using ViaTrade.Application.Users.GetCurrent;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class UsersController(ISender sender, IAuthCookieService authCookieService, ILogger<UsersController> logger)
	: ControllerBase
{
	[HttpPost]
	[AllowAnonymous]
	[SetsAuthCookies]
	public async Task<NoContent> Register([FromBody] RegisterCommand command, CancellationToken ct)
	{
		await sender.Send(command, ct);

		var userAgent = Request.Headers.UserAgent.ToString();

		var loginCommand = new LoginCommand(command.Login, command.Password, userAgent);
		var tokens = await sender.Send(loginCommand, ct);

		authCookieService.SetAuthCookies(Response, tokens);
		return TypedResults.NoContent();
	}

	[HttpGet("me")]
	public async Task<Results<Ok<CurrentUserResult>, NotFound>> GetMe(CancellationToken ct)
	{
		logger.LogInformation("Getting current user information");

		var user = await sender.Send(new GetCurrentUserQuery(), ct);

		return TypedResults.Ok(user);
	}

	[HttpPost("me/telegramLinkToken")]
	public async Task<Ok<TelegramLinkResult>> GenerateTelegramToken(CancellationToken ct)
	{
		logger.LogInformation("Generating Telegram token for user");

		var token = await sender.Send(new CreateTelegramLinkCommand(), ct);

		return TypedResults.Ok(token);
	}
}
