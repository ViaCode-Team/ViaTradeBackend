using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Auth.Models;
using Application.Common.Models;
using Infrastructure.Configuration;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ViaTradeBackend.Contracts.Auth;
using ViaTradeBackend.Contracts.Users;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
	IAuthCommandService authCommandService,
	IAuthQueryService authQueryService,
	IJwtHelper jwtHelper,
	IOptions<AuthCookieOptions> authOptions
) : ControllerBase
{
	private readonly AuthCookieOptions _authCookiOptions = authOptions.Value;

	[HttpPost("login")]
	public async Task<NoContent> Login([FromBody, Required] LoginRequest request, CancellationToken ct)
	{
		var userAgent = Request.Headers.UserAgent.ToString();
		var result = await authCommandService.LoginAsync(request.Login, request.Password, userAgent, ct);

		SetAuthCookies(result);
		return TypedResults.NoContent();
	}

	[HttpPost("register")]
	public async Task<Created> Register([FromBody, Required] RegisterRequest request, CancellationToken ct)
	{
		var userAgent = Request.Headers.UserAgent.ToString();
		var result = await authCommandService.RegisterAsync(request.Login, request.Password, userAgent, ct);

		SetAuthCookies(result);
		return TypedResults.Created();
	}

	[HttpPost("refresh")]
	public async Task<NoContent> RefreshToken(CancellationToken ct)
	{
		if (!Request.Cookies.TryGetValue(_authCookiOptions.RefreshTokenCookie, out var refreshToken))
			throw new UnauthorizedAccessException();

		var result = await authCommandService.RefreshTokenAsync(refreshToken, ct);

		SetAuthCookies(result);
		return TypedResults.NoContent();
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<NoContent> Logout(CancellationToken ct)
	{
		if (Request.Cookies.TryGetValue(_authCookiOptions.RefreshTokenCookie, out var refreshToken))
		{
			await authCommandService.LogoutSessionAsync(refreshToken, ct);
		}

		Response.Cookies.Delete(_authCookiOptions.AccessTokenCookie);
		Response.Cookies.Delete(_authCookiOptions.RefreshTokenCookie);

		return TypedResults.NoContent();
	}

	[HttpPost("logout-all")]
	[Authorize]
	public async Task<NoContent> LogoutAll(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await authCommandService.LogoutAllAsync(userId, ct);

		Response.Cookies.Delete(_authCookiOptions.AccessTokenCookie);
		Response.Cookies.Delete(_authCookiOptions.RefreshTokenCookie);

		return TypedResults.NoContent();
	}

	[HttpGet("sessions")]
	[Authorize]
	public async Task<Ok<PageResult<UserSessionResponse>>> GetUserSessions(
		[FromQuery] PageOptions page,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userSessions = await authQueryService.GetSessionsPagedAsync(userId, page, ct);

		return TypedResults.Ok(userSessions.Map(s => s.Adapt<UserSessionResponse>()));
	}

	private void SetAuthCookies(AuthTokens result)
	{
		Response.Cookies.Append(
			_authCookiOptions.AccessTokenCookie,
			result.AccessToken,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTimeOffset.UtcNow.AddHours(1),
				Path = "/",
			}
		);

		Response.Cookies.Append(
			_authCookiOptions.RefreshTokenCookie,
			result.RefreshToken,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTimeOffset.UtcNow.AddDays(7),
				Path = "/",
			}
		);
	}
}
