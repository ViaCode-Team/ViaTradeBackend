using Microsoft.Extensions.Options;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Configuration.Options;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Email;

public sealed class EmailSubmissionService(
	IEmailQueue queue,
	IPasswordHasher passwordHasher,
	IReadRepository<User> users,
	ISessionRepository sessions,
	IOptions<EmailSettings> options
)
{
	public async Task<EmailAcceptedResult> RegisterAsync(
		string login,
		string email,
		string password,
		CancellationToken ct
	)
	{
		EmailIdentity.ValidatePassword(password);
		return await queue.PublishAsync(
			NewJob(EmailAction.Register) with
			{
				Login = login.Trim(),
				Email = email.Trim(),
				PasswordHash = passwordHasher.Hash(password),
			},
			ct
		);
	}

	public Task<EmailAcceptedResult> RequestAsync(EmailAction action, string identity, CancellationToken ct)
	{
		return queue.PublishAsync(NewJob(action) with { Login = identity.Trim() }, ct);
	}

	public async Task<EmailAcceptedResult> TokenAsync(
		int? userId,
		string? sessionId,
		EmailAction action,
		string token,
		string? password,
		CancellationToken ct
	)
	{
		string? passwordHash = null;
		if (password != null)
		{
			EmailIdentity.ValidatePassword(password);
			passwordHash = passwordHasher.Hash(password);
		}

		var fingerprint = EmailIdentity.Fingerprint(token);
		var record = await queue.FindTokenAsync(fingerprint, ct);
		var job = NewJob(action) with
		{
			TokenFingerprint = fingerprint,
			PasswordHash = passwordHash,
			UserId = record?.UserId,
		};
		if (userId != null)
			job = job with { UserId = userId, SessionId = sessionId };

		return await queue.PublishAsync(job, ct);
	}

	public async Task<EmailAcceptedResult> StartChangeAsync(
		int userId,
		string sessionId,
		string email,
		string password,
		CancellationToken ct
	)
	{
		EmailIdentity.ValidatePassword(password);
		if (!EmailIdentity.IsEmail(email.Trim()))
			throw new BadRequestException("A valid email address is required.", "invalid_email");
		var user = await users.FirstOrDefaultAsync(user => user.Id == userId, ct);
		if (user == null || !passwordHasher.Verify(password, user.PasswordHash))
			throw new InvalidCredentialsException();

		var session = await sessions.FindByIdAsync(sessionId);
		if (
			session == null
			|| session.UserId != userId
			|| session.AuthorizationVersion != user.AuthorizationVersion
			|| session.ExpiresAt <= DateTime.UtcNow
		)
			throw new AuthenticationException();

		return await queue.PublishAsync(
			NewJob(EmailAction.StartEmailChange) with
			{
				UserId = userId,
				SessionId = sessionId,
				Email = email.Trim(),
				VerifiedAuthorizationVersion = user.AuthorizationVersion,
			},
			ct
		);
	}

	private EmailJob NewJob(EmailAction action)
	{
		var now = EmailIdentity.UtcNow();
		return new EmailJob
		{
			Id = Guid.NewGuid(),
			Action = action,
			CreatedAt = now,
			ExpiresAt = now.AddMinutes(options.Value.JobWaitMinutes),
		};
	}
}
