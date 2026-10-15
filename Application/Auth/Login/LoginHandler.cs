using Mediator;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Email;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Auth.Login;

public sealed class LoginHandler(
	IRepository<User> userRepository,
	IPasswordHasher passwordHasher,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	AuthTokenFactory tokenFactory,
	AccountSessionValidator accountSessions
) : ICommandHandler<LoginCommand, AuthTokensResult>
{
	public async ValueTask<AuthTokensResult> Handle(LoginCommand command, CancellationToken ct)
	{
		var identity = EmailIdentity.NormalizeLogin(command.Login);
		var byEmail = command.Login.Contains('@');
		var user = await userRepository.FirstOrDefaultAsync(
			user => byEmail && user.NormalizedEmail == identity || !byEmail && user.NormalizedLogin == identity,
			user => new UserCredentials(
				user.Id,
				user.Login,
				user.PasswordHash,
				user.AuthorizationVersion,
				user.Email != null && user.EmailConfirmedAt != null
			),
			ct
		);

		if (user == null || !passwordHasher.Verify(command.Password, user.PasswordHash))
			throw new InvalidCredentialsException();

		if (!user.EmailConfirmed)
			throw new ForbiddenException("Confirm your email before signing in.", "email_confirmation_required");

		var now = DateTime.UtcNow;
		var sessionId = Guid.NewGuid().ToString();
		var session = new SessionData
		{
			Id = sessionId,
			UserId = user.Id,
			AuthorizationVersion = user.AuthorizationVersion,
			UserAgent = command.UserAgent,
			CreatedAt = now,
			LastSeen = now,
			ExpiresAt = tokenFactory.CalculateExpiresAt(now, now),
		};

		var sessionTtl = session.ExpiresAt - now;
		var refreshToken = jwtHelper.GenerateRefreshToken();

		await sessionRepository.CreateSessionAsync(session, refreshToken, sessionTtl);

		if (!await accountSessions.IsCurrentAsync(session, ct))
		{
			await sessionRepository.TerminateSessionAsync(session.Id);
			throw new InvalidCredentialsException();
		}

		return tokenFactory.CreateAuthTokens(new TokenUser(user.Id, user.Login), session, refreshToken, now);
	}
}
