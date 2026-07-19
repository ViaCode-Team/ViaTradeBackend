using Application.Auth.Commands;
using Application.Auth.Interfaces;
using Application.Auth.Queries;
using Application.Common.Models;
using Application.Common.Models.Pagination;
using Infrastructure.Configuration;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Auth;
using ViaTradeBackend.Contracts.Users;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(ISender sender, IJwtHelper jwtHelper, IOptions<AuthCookieOptions> authOptions) : ControllerBase
{
	private readonly AuthCookieOptions _authCookiOptions = authOptions.Value;

	[HttpPost("login")]
	public async Task<NoContent> Login(
		[FromBody, Required] LoginRequest request,
		CancellationToken cancellationToken)
	{
		var userAgent = Request.Headers.UserAgent.ToString();
		var command = new LoginCommand(request.Login, request.Password, userAgent);
		var result = await sender.Send(command, cancellationToken);

		SetAuthCookies(result);
		return TypedResults.NoContent();
	}

	[HttpPost("register")]
	public async Task<Created> Register(
		[FromBody, Required] RegisterRequest request,
		CancellationToken cancellationToken)
	{
		var userAgent = Request.Headers.UserAgent.ToString();
		var command = new RegisterCommand(request.Login, request.Password, userAgent);
		var result = await sender.Send(command, cancellationToken);

		SetAuthCookies(result);
		return TypedResults.Created();
	}

	[HttpPost("refresh")]
	public async Task<NoContent> RefreshToken(CancellationToken cancellationToken)
	{
		if (!Request.Cookies.TryGetValue(_authCookiOptions.RefreshTokenCookie, out var refreshToken))
			throw new UnauthorizedAccessException();

		var command = new RefreshTokenCommand(refreshToken);
		var result = await sender.Send(command, cancellationToken);

		SetAuthCookies(result);
		return TypedResults.NoContent();
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<NoContent> Logout()
	{
		if (Request.Cookies.TryGetValue(_authCookiOptions.RefreshTokenCookie, out var refreshToken))
		{
			var command = new LogoutSessionCommand(refreshToken);
			await sender.Send(command);
		}

		Response.Cookies.Delete(_authCookiOptions.AccessTokenCookie);
		Response.Cookies.Delete(_authCookiOptions.RefreshTokenCookie);

		return TypedResults.NoContent();
	}

	[HttpPost("logout-all")]
	[Authorize]
	public async Task<NoContent> LogoutAll()
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new LogoutAllCommand(userId);
		await sender.Send(command);

		Response.Cookies.Delete(_authCookiOptions.AccessTokenCookie);
		Response.Cookies.Delete(_authCookiOptions.RefreshTokenCookie);

		return TypedResults.NoContent();
	}

	[HttpGet("sessions")]
	[Authorize]
	public async Task<Ok<PagedResult<UserSessionResponse>>> GetUserSessions([FromQuery] PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetPagedUserSessionsQuery(userId, paginationRequest);
		var pagedSessions = await sender.Send(query, cancellationToken);

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
