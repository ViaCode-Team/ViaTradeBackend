using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Auth;
using ViaTrade.Api.Contracts.Users;
using ViaTrade.Api.Cookies;
using ViaTrade.Api.Mappings;
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
		[FromBody, Required] RegisterRequest request,
		[FromServices] ICommandHandler<RegisterCommand, AuthTokensResult> handler,
		CancellationToken ct
	)
	{
		var userAgent = Request.Headers.UserAgent.ToString();
		var command = new RegisterCommand(request.Login, request.Password, userAgent);
		var tokens = await handler.HandleAsync(command, ct);

		authCookieService.SetAuthCookies(Response, tokens);
		return TypedResults.NoContent();
	}

	[HttpGet("me")]
	public async Task<Results<Ok<UserMeResponse>, NotFound>> GetMe(
		[FromServices] IQueryHandler<GetCurrentUserQuery, CurrentUserResult> handler,
		CancellationToken ct
	)
	{
		logger.LogInformation("Getting current user information");

		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetCurrentUserQuery(userId);
		var user = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(user));
	}

	[HttpPost("me/telegramLinkToken")]
	public async Task<Ok<TelegramTokenResponse>> GenerateTelegramToken(
		[FromServices] ICommandHandler<CreateTelegramLinkCommand, TelegramLinkResult> handler,
		CancellationToken ct
	)
	{
		logger.LogInformation("Generating Telegram token for user");

		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new CreateTelegramLinkCommand(userId);
		var token = await handler.HandleAsync(command, ct);

		var response = new TelegramTokenResponse(token.TelegramToken);

		return TypedResults.Ok(response);
	}
}
