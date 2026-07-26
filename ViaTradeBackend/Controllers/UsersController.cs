using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Users.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Users;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(
	IUserCommandService userCommandService,
	IUserQueryService userQueryService,
	IJwtHelper jwtHelper,
	ILogger<UsersController> logger
) : ControllerBase
{
	[HttpGet("me")]
	public async Task<Results<Ok<UserMeResponse>, NotFound>> GetMe(CancellationToken ct)
	{
		logger.LogInformation("Getting current user information");

		var userId = jwtHelper.GetUserIdFromClaims(User);
		var user = await userQueryService.GetCurrentUserAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(user));
	}

	[HttpGet("telegramToken")]
	public async Task<Ok<TelegramTokenResponse>> GenerateTelegramToken(CancellationToken ct)
	{
		logger.LogInformation("Generating Telegram token for user");

		var userId = jwtHelper.GetUserIdFromClaims(User);
		var token = await userCommandService.GenerateTgLinkAsync(userId, ct);
		var response = new TelegramTokenResponse(token);

		return TypedResults.Ok(response);
	}

	[ServicePassword]
	[HttpPost("telegramToken")]
	public async Task<Accepted> LinkTelegramToken(
		[FromBody, Required] LinkTelegramRequest request,
		CancellationToken ct
	)
	{
		logger.LogInformation("Processing Telegram token for user");

		await userCommandService.LinkTelegramAsync(request.TelegramToken, request.TelegramId, ct);

		logger.LogInformation("Telegram token processed successfully");
		return TypedResults.Accepted(string.Empty);
	}

	[ServicePassword]
	[HttpGet("user")]
	public async Task<Ok<List<UserTelegramResponse>>> GetUsersWithTgLink(CancellationToken ct)
	{
		logger.LogInformation("Getting all users with Telegram links");

		var users = await userQueryService.ListTelegramRecipientsAsync(ct);

		return TypedResults.Ok(users.Select(ApiMapper.ToResponse).ToList());
	}
}
