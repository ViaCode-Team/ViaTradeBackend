using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Auth.Register;
using ViaTrade.Application.Email;
using ViaTrade.Application.Users.CreateTelegramLink;
using ViaTrade.Application.Users.GetCurrent;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class UsersController(ISender sender, EmailRequestLimiter limiter, ILogger<UsersController> logger)
	: ControllerBase
{
	[HttpPost]
	[AllowAnonymous]
	public async Task<Accepted<EmailAcceptedResult>> Register([FromBody] RegisterCommand command, CancellationToken ct)
	{
		await limiter.RequireIpAsync(HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", ct);
		var result = await sender.Send(command, ct);
		return TypedResults.Accepted($"/{ApiRoutes.V1.Web}/users/operations/{result.OperationId}", result);
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
