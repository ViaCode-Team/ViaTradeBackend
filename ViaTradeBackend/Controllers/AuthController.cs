using Application.Interfaces.Services;
using Application.Interfaces.Utils;
using Domain.Models;
using Domain.Models.ConfigOptions;
using Domain.Models.Dto.User;
using Domain.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Models.Auth;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
	IAuthService authService,
	IJwtHelper jwtHelper,
	IOptions<AuthCookiOptions> authOptions) : ControllerBase
{
	private readonly IAuthService _authService = authService;
	private readonly IJwtHelper _jwtHelper = jwtHelper;
	private readonly AuthCookiOptions _authCookiOptions = authOptions.Value;

	[HttpPost("login")]
	public async Task<ActionResult> Login(
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
		return NoContent();
	}

	[HttpPost("register")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<ActionResult> Register(
		[FromBody, Required] RegisterRequest request,
		CancellationToken cancellationToken)
	{
		var result = await _authService.RegisterAsync(
			request.Login,
			request.Password,
			cancellationToken);

		SetAuthCookies(result);
		return Created();
	}

	[HttpPost("refresh")]
	public async Task<ActionResult> RefreshToken(
		CancellationToken cancellationToken)
	{
		if (!Request.Cookies.TryGetValue(_authCookiOptions.RefreshTokenCookie, out var refreshToken))
			throw new UnauthorizedAccessException();

		var result = await _authService.RefreshTokenAsync(
			refreshToken,
			cancellationToken);

		SetAuthCookies(result);
		return NoContent();
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<ActionResult> Logout()
	{
		if (Request.Cookies.TryGetValue(_authCookiOptions.RefreshTokenCookie, out var refreshToken))
			await _authService.LogoutSessionAsync(refreshToken);

		Response.Cookies.Delete(_authCookiOptions.AccessTokenCookie);
		Response.Cookies.Delete(_authCookiOptions.RefreshTokenCookie);

		return NoContent();
	}

	[HttpPost("logout-all")]
	[Authorize]
	public async Task<ActionResult> LogoutAll()
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);

		await _authService.LogoutAllAsync(userId);

		Response.Cookies.Delete(_authCookiOptions.AccessTokenCookie);
		Response.Cookies.Delete(_authCookiOptions.RefreshTokenCookie);

		return NoContent();
	}

	[HttpGet("sessions")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<UserSessionDto>>> GetUserSessions([FromQuery] PaginationRequest paginationRequest)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);

		var pagedSessions = await _authService.GetPagedUserSessionsAsync(userId, paginationRequest);

		var result = pagedSessions.Map(s => new UserSessionDto
		{
			Id = s.Id,
			UserAgent = s.UserAgent,
			CreatedAt = s.CreatedAt,
			LastSeen = s.LastSeen
		});

		return Ok(result);
	}

	private void SetAuthCookies(AuthResult result)
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
