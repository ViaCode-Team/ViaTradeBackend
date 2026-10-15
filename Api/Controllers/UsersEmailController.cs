using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Security.Authentication.Cookies;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Email;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/users")]
[ApiController]
public sealed class UsersEmailController(
	EmailSubmissionService submission,
	IEmailQueue queue,
	EmailRequestLimiter limiter,
	IUserContext currentUser,
	IOptionalUserContext optionalUser,
	IAuthCookieService cookies
) : ControllerBase
{
	[HttpPost("emailConfirmation")]
	[AllowAnonymous]
	public Task<Accepted<EmailAcceptedResult>> ConfirmEmail(
		[FromBody] EmailTokenRequest request,
		CancellationToken ct
	) => TokenAsync(EmailAction.ConfirmRegistration, request.Token, null, false, ct);

	[HttpPost("emailConfirmation/resend")]
	[AllowAnonymous]
	public Task<Accepted<EmailAcceptedResult>> ResendConfirmation(
		[FromBody] EmailIdentityRequest request,
		CancellationToken ct
	) => RequestAsync(EmailAction.ResendConfirmation, request.Login, ct);

	[HttpPost("passwordReset/request")]
	[AllowAnonymous]
	public Task<Accepted<EmailAcceptedResult>> RequestPasswordReset(
		[FromBody] EmailIdentityRequest request,
		CancellationToken ct
	) => RequestAsync(EmailAction.RequestPasswordReset, request.Login, ct);

	[HttpPost("passwordReset")]
	[AllowAnonymous]
	public Task<Accepted<EmailAcceptedResult>> ResetPassword(
		[FromBody] EmailPasswordRequest request,
		CancellationToken ct
	) => TokenAsync(EmailAction.ResetPassword, request.Token, request.NewPassword, false, ct);

	[HttpPost("me/emailChange")]
	public async Task<Accepted<EmailAcceptedResult>> StartEmailChange(
		[FromBody] EmailChangeRequest request,
		CancellationToken ct
	)
	{
		await LimitAsync(false, ct);
		await limiter.RequireAccountAsync(currentUser.UserId, ct);
		var result = await submission.StartChangeAsync(
			currentUser.UserId,
			currentUser.SessionId,
			request.Email,
			request.CurrentPassword,
			ct
		);
		return Accepted(result);
	}

	[HttpPost("me/emailChange/confirm")]
	public Task<Accepted<EmailAcceptedResult>> ConfirmEmailChange(
		[FromBody] EmailTokenRequest request,
		CancellationToken ct
	) => TokenAsync(EmailAction.ConfirmEmailChange, request.Token, null, true, ct);

	[HttpPost("emailChangeRecovery")]
	[AllowAnonymous]
	public Task<Accepted<EmailAcceptedResult>> RecoverEmail(
		[FromBody] EmailPasswordRequest request,
		CancellationToken ct
	) => TokenAsync(EmailAction.RecoverEmail, request.Token, request.NewPassword, false, ct);

	[HttpGet("operations/{operationId:guid}")]
	[AllowAnonymous]
	public async Task<Results<Ok<EmailOperationResult>, NotFound>> GetOperation(
		Guid operationId,
		[FromHeader(Name = "X-Operation-Key"), Required, StringLength(64, MinimumLength = 64)] string accessKey,
		CancellationToken ct
	)
	{
		await LimitAsync(false, ct);
		Response.Headers.CacheControl = "no-store";
		var result = await queue.FindResultAsync(
			optionalUser.UserId,
			operationId,
			optionalUser.SessionId,
			accessKey,
			ct
		);
		if (result == null)
			return TypedResults.NotFound();

		if (result.ClearCookies)
			cookies.DeleteAuthCookies(Response);
		return TypedResults.Ok(result);
	}

	private async Task<Accepted<EmailAcceptedResult>> RequestAsync(
		EmailAction action,
		string identity,
		CancellationToken ct
	)
	{
		await LimitAsync(false, ct);
		return Accepted(await submission.RequestAsync(action, identity, ct));
	}

	private async Task<Accepted<EmailAcceptedResult>> TokenAsync(
		EmailAction action,
		string token,
		string? password,
		bool authorized,
		CancellationToken ct
	)
	{
		await LimitAsync(true, ct);
		int? userId = null;
		string? sessionId = null;
		if (authorized)
		{
			userId = currentUser.UserId;
			sessionId = currentUser.SessionId;
			await limiter.RequireAccountAsync(userId.Value, ct);
		}
		return Accepted(await submission.TokenAsync(userId, sessionId, action, token, password, ct));
	}

	private async Task LimitAsync(bool token, CancellationToken ct)
	{
		var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		await limiter.RequireIpAsync(ip, ct);
		if (token)
			await limiter.RequireTokenAsync(ip, ct);
	}

	private static Accepted<EmailAcceptedResult> Accepted(EmailAcceptedResult result) =>
		TypedResults.Accepted($"/{ApiRoutes.V1.Web}/users/operations/{result.OperationId}", result);
}

public sealed record EmailIdentityRequest([Required, StringLength(254, MinimumLength = 1)] string Login);

public sealed record EmailTokenRequest([Required, RegularExpression("^[A-Fa-f0-9]{64}$")] string Token);

public sealed record EmailPasswordRequest(
	[Required, RegularExpression("^[A-Fa-f0-9]{64}$")] string Token,
	[Required, StringLength(72, MinimumLength = 8)] string NewPassword
);

public sealed record EmailChangeRequest(
	[Required, EmailAddress, StringLength(254)] string Email,
	[Required, StringLength(72, MinimumLength = 8)] string CurrentPassword
);
