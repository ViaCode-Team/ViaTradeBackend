using Application.Auth.Interfaces;
using Application.Users.Commands;
using Application.Users.Queries;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Users;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(ISender sender, IJwtHelper jwtHelper, ILogger<UserController> logger) : ControllerBase
{
	[Authorize]
	[HttpGet("me")]
	public async Task<Results<Ok<UserMeResponse>, NotFound>> GetMe(CancellationToken ct)
	{
		logger.LogInformation("Getting current user information");

		var userId = jwtHelper.GetUserIdFromClaims(User);

		var query = new GetUserByIdQuery(userId);
		var user = await sender.Send(query, ct);

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

		var command = new GenerateTgLinkCommand(userId);
		var response = new TgTokenResponse(await sender.Send(command, ct));

		return TypedResults.Ok(response);
	}

	[ServicePassword]
	[HttpPost("tgToken")]
	public async Task<Accepted> LinkTgToken(
		[FromBody, Required] LinkTelegramRequest request,
		CancellationToken ct)
	{
		logger.LogInformation("Processing Telegram token for user");

		var command = new LinkTelegramCommand(request.TgToken, request.TgId);
		await sender.Send(command, ct);

		logger.LogInformation("Telegram token processed successfully");
		return TypedResults.Accepted(string.Empty);
	}

	[ServicePassword]
	[HttpGet("user")]
	public async Task<Ok<List<UserTgResponse>>> GetUsersWithTgLink(CancellationToken ct)
	{
		logger.LogInformation("Getting all users with Telegram links");

		var query = new GetAllUsersWithTgLinkQuery();
		var users = await sender.Send(query, ct);

		return TypedResults.Ok(users.Adapt<List<UserTgResponse>>());
	}
}
