using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Auth;
using ViaTradeBackend.Contracts.Users;
using ViaTradeBackend.Cookies;
using ViaTradeBackend.Mappings;
using ViaTradeBackend.Swagger.Attributes;

namespace ViaTradeBackend.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class UsersController(
	IAuthCommandService authCommandService,
	IUserCommandService userCommandService,
	IUserQueryService userQueryService,
	IJwtHelper jwtHelper,
	IAuthCookieService authCookieService,
	ILogger<UsersController> logger
) : ControllerBase
{
	[HttpPost]
	[AllowAnonymous]
	[SetsAuthCookies]
	public async Task<NoContent> Register([FromBody, Required] RegisterRequest request, CancellationToken ct)
	{
		var userAgent = Request.Headers.UserAgent.ToString();
		var tokens = await authCommandService.RegisterAsync(request.Login, request.Password, userAgent, ct);

		authCookieService.SetAuthCookies(Response, tokens);
		return TypedResults.NoContent();
	}

	[HttpGet("me")]
	public async Task<Results<Ok<UserMeResponse>, NotFound>> GetMe(CancellationToken ct)
	{
		logger.LogInformation("Getting current user information");

		var userId = jwtHelper.GetUserIdFromClaims(User);
		var user = await userQueryService.GetCurrentUserAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(user));
	}

	[HttpPost("me/telegramLinkToken")]
	public async Task<Ok<TelegramTokenResponse>> GenerateTelegramToken(CancellationToken ct)
	{
		logger.LogInformation("Generating Telegram token for user");

		var userId = jwtHelper.GetUserIdFromClaims(User);
		var token = await userCommandService.GenerateTgLinkAsync(userId, ct);

		var response = new TelegramTokenResponse(token);

		return TypedResults.Ok(response);
	}
}
