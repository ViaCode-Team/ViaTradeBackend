using Application.Auth.Interfaces;
using Application.Users.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Users;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(
	IUserCommandService userCommandService,
	IUserQueryService userQueryService,
	IJwtHelper jwtHelper,
	ILogger<UsersController> logger) : ControllerBase
{
	[Authorize]
	[HttpGet("me")]
	public async Task<Results<Ok<UserMeResponse>, NotFound>> GetMe(CancellationToken ct)
	{
		logger.LogInformation("Getting current user information");

		var userId = jwtHelper.GetUserIdFromClaims(User);
		var user = await userQueryService.GetAsync(userId, ct);

		if (user == null)
			return TypedResults.NotFound();

		return TypedResults.Ok(user.Adapt<UserMeResponse>());
	}

	[Authorize]
	[HttpGet("tgToken")]
	public async Task<Ok<TgTokenResponse>> GenerateTgToken(CancellationToken ct)
	{
		logger.LogInformation("Generating Telegram token for user");

		var userId = jwtHelper.GetUserIdFromClaims(User);
		var token = await userCommandService.GenerateTgLinkAsync(userId, ct);
		var response = new TgTokenResponse(token);

		return TypedResults.Ok(response);
	}

	[ServicePassword]
	[HttpPost("tgToken")]
	public async Task<Accepted> LinkTgToken(
		[FromBody, Required] LinkTelegramRequest request,
		CancellationToken ct)
	{
		logger.LogInformation("Processing Telegram token for user");

		await userCommandService.LinkTelegramAsync(request.TgToken, request.TgId, ct);

		logger.LogInformation("Telegram token processed successfully");
		return TypedResults.Accepted(string.Empty);
	}

	[ServicePassword]
	[HttpGet("user")]
	public async Task<Ok<List<UserTgResponse>>> GetUsersWithTgLink(CancellationToken ct)
	{
		logger.LogInformation("Getting all users with Telegram links");

		var users = await userQueryService.GetWithTgLinkAsync(ct);

		return TypedResults.Ok(users.Adapt<List<UserTgResponse>>());
	}
}
