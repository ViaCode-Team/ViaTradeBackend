using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Entities.DataBase;
using Domain.Models.Dto.User;
using Domain.Models.Request.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Attribute;

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
	public async Task<ActionResult<MeDto>> GetMe(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting current user information");
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var user = await _userService.EnsureUserAsync(userId, cancellationToken);

		return Ok(new MeDto
		{
			Id = user.Id,
			Login = user.Login,
			LastLoginDate = user.LastLoginDate,
			RegisterDate = user.RegisterDate,
			TgId = user.TgId
		});
	}

	[Authorize]
	[HttpGet("tgToken")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<TgTokenResponse>> GenerateTgToken(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Generating Telegram token for user");
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var user = await _userService.EnsureUserAsync(userId, cancellationToken);

		var response = new TgTokenResponse
		{
			TgToken = await _userService.GenerateTgLink(user.Id)
		};

		return Ok(response);
	}

	[ServicePassword]
	[HttpPost("tgToken")]
	[ProducesResponseType(StatusCodes.Status202Accepted)]
	public async Task<ActionResult> LinkTgToken(
		[FromBody, Required] TgTokenRequest tgTokenRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Processing Telegram token for user");
		await _userService.LinkTelegramAsync(tgTokenRequest.TgToken, tgTokenRequest.TgId, cancellationToken);

		_logger.LogInformation("Telegram token processed successfully");
		return Accepted();
	}

	[ServicePassword]
	[HttpGet("user")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<User>>> GetUsersWithTgLink(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting all users with Telegram links");
		return Ok(await _userService.GetAllWithTgLinkAsync(cancellationToken));
	}
}
