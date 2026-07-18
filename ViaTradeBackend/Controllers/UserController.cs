using Application.Interfaces.Utils;
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
using ViaTradeBackend.Contracts.Auth;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(
	ISender sender,
	IJwtHelper jwtHelper,
	ILogger<UserController> logger) : ControllerBase
{
	private readonly ISender _sender = sender;
	private readonly IJwtHelper _jwtHelper = jwtHelper;
	private readonly ILogger<UserController> _logger = logger;

	[Authorize]
	[HttpGet("me")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Results<Ok<UserMeResponse>, NotFound>> GetMe(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting current user information");
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserByIdQuery(userId);
		var user = await _sender.Send(query, cancellationToken);
		
		if (user == null)
			return TypedResults.NotFound();

		return TypedResults.Ok(user.Adapt<UserMeResponse>());
	}

	[Authorize]
	[HttpGet("tgToken")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TgTokenResponse>> GenerateTgToken(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Generating Telegram token for user");
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new GenerateTgLinkCommand(userId);
		var response = new TgTokenResponse(await _sender.Send(command, cancellationToken));

		return TypedResults.Ok(response);
	}

	[ServicePassword]
	[HttpPost("tgToken")]
	[ProducesResponseType(StatusCodes.Status202Accepted)]
	public async Task<Accepted> LinkTgToken(
		[FromBody, Required] LinkTelegramRequest request,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Processing Telegram token for user");
		var command = new LinkTelegramCommand(request.TgToken, request.TgId);
		await _sender.Send(command, cancellationToken);

		_logger.LogInformation("Telegram token processed successfully");
		return TypedResults.Accepted(string.Empty);
	}

	[ServicePassword]
	[HttpGet("user")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<List<UserTgResponse>>> GetUsersWithTgLink(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting all users with Telegram links");
		var query = new GetAllUsersWithTgLinkQuery();
		var users = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(users.Adapt<List<UserTgResponse>>());
	}
}
