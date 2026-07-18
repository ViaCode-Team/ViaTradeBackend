using Application.Interfaces;
using Application.Interfaces.Utils;
using Mapster;
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
	IJwtHelper jwtHelper,
	IUserService userService,
	ILogger<UserController> logger) : ControllerBase
{
	private readonly IJwtHelper _jwtHelper = jwtHelper;
	private readonly IUserService _userService = userService;
	private readonly ILogger<UserController> _logger = logger;

	[Authorize]
	[HttpGet("me")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Results<Ok<UserMeResponse>, NotFound>> GetMe(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting current user information");
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var user = await _userService.GetByIdAsync(userId, cancellationToken);
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

		var response = new TgTokenResponse(await _userService.GenerateTgLink(userId));

		return TypedResults.Ok(response);
	}

	[ServicePassword]
	[HttpPost("tgToken")]
	[ProducesResponseType(StatusCodes.Status202Accepted)]
	public async Task<Accepted> LinkTgToken(
		[FromBody, Required] LinkTelegramRequest LinkTelegramRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Processing Telegram token for user");
		await _userService.LinkTelegramAsync(LinkTelegramRequest.TgToken, LinkTelegramRequest.TgId, cancellationToken);

		_logger.LogInformation("Telegram token processed successfully");
		return TypedResults.Accepted(string.Empty);
	}

	[ServicePassword]
	[HttpGet("user")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<List<UserTgResponse>>> GetUsersWithTgLink(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting all users with Telegram links");
		var users = await _userService.GetAllWithTgLinkAsync(cancellationToken);
		return TypedResults.Ok(users.Adapt<List<UserTgResponse>>());
	}
}


