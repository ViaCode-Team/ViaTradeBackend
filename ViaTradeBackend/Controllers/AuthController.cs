using Application.Interfaces.Services;
using Application.Interfaces.Utils;
using Application.Models;
using Domain.Models.ConfigOptions;
using Domain.Models.Pagination;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Users;

using ViaTradeBackend.Contracts.Auth;

namespace ViaTradeBackend.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController(
	IAuthService authService,
	IJwtHelper jwtHelper,
	IOptions<AuthCookieOptions> authOptions) : ControllerBase
{
	private readonly IAuthService _authService = authService;
	private readonly IJwtHelper _jwtHelper = jwtHelper;
	private readonly AuthCookieOptions _authCookiOptions = authOptions.Value;

	[HttpPost("login")]
	public async Task<NoContent> Login(
		[FromBody, Required] LoginRequest request,
		CancellationToken cancellationToken)
	{
		var userAgent = Request.Headers.UserAgent.ToString();

		var result = await _authService.LoginAsync(
			request.Login,
			request.Password,
			userAgent,
			cancellationToken);

		SetAuthCookies(result);
		return TypedResults.NoContent();
	}

	[HttpPost("register")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> Register(
		[FromBody, Required] RegisterRequest request,
		CancellationToken cancellationToken)
	{
		var result = await _authService.RegisterAsync(
			request.Login,
			request.Password,
			cancellationToken);

		SetAuthCookies(result);
		return TypedResults.Created();
	}

	[HttpPost("refresh")]
	public async Task<NoContent> RefreshToken(CancellationToken cancellationToken)
	{
		if (!Request.Cookies.TryGetValue(_authCookiOptions.RefreshTokenCookie, out var refreshToken))
			throw new UnauthorizedAccessException();

		var result = await _authService.RefreshTokenAsync(
			refreshToken,
			cancellationToken);

		SetAuthCookies(result);
		return TypedResults.NoContent();
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<NoContent> Logout()
	{
		if (Request.Cookies.TryGetValue(_authCookiOptions.RefreshTokenCookie, out var refreshToken))
			await _authService.LogoutSessionAsync(refreshToken);

		Response.Cookies.Delete(_authCookiOptions.AccessTokenCookie);
		Response.Cookies.Delete(_authCookiOptions.RefreshTokenCookie);

		return TypedResults.NoContent();
	}

	[HttpPost("logout-all")]
	[Authorize]
	public async Task<NoContent> LogoutAll()
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);

		await _authService.LogoutAllAsync(userId);

		Response.Cookies.Delete(_authCookiOptions.AccessTokenCookie);
		Response.Cookies.Delete(_authCookiOptions.RefreshTokenCookie);

		return TypedResults.NoContent();
	}

	[HttpGet("sessions")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<UserSessionResponse>>> GetUserSessions([FromQuery] PaginationRequest paginationRequest)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);

		var pagedSessions = await _authService.GetPagedUserSessionsAsync(userId, paginationRequest);

		return TypedResults.Ok(pagedSessions.Map(s => s.Adapt<UserSessionResponse>()));
	}

	private void SetAuthCookies(AuthInternalResult result)
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
				Path = "/"
			});

		Response.Cookies.Append(
			_authCookiOptions.RefreshTokenCookie,
			result.RefreshToken,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTimeOffset.UtcNow.AddDays(7),
				Path = "/"
			});
	}
}

