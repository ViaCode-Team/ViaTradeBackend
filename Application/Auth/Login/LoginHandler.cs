using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Auth.Login;

public sealed class LoginHandler(
	IRepository<User> userRepository,
	IPasswordHasher passwordHasher,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	AuthTokenFactory tokenFactory
) : ICommandHandler<LoginCommand, AuthTokensResult>
{
	public async Task<AuthTokensResult> HandleAsync(LoginCommand command, CancellationToken ct = default)
	{
		var user = await userRepository.FirstOrDefaultAsync(
			user => user.Login == command.Login,
			user => new UserCredentials(user.Id, user.Login, user.PasswordHash),
			ct
		);

		if (user == null || !passwordHasher.Verify(command.Password, user.PasswordHash))
			throw new InvalidCredentialsException();

		var now = DateTime.UtcNow;
		var sessionId = Guid.NewGuid().ToString();
		var session = new SessionData
		{
			Id = sessionId,
			UserId = user.Id,
			UserAgent = command.UserAgent,
			CreatedAt = now,
			LastSeen = now,
			ExpiresAt = tokenFactory.CalculateExpiresAt(now, now),
		};
		var sessionTtl = session.ExpiresAt - now;
		var refreshToken = jwtHelper.GenerateRefreshToken();

		await sessionRepository.CreateSessionAsync(session, refreshToken, sessionTtl);

		return tokenFactory.CreateAuthTokens(new TokenUser(user.Id, user.Login), session, refreshToken, now);
	}
}
