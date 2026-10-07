using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Auth.RefreshTokens;

public sealed class RefreshTokensHandler(
	IRepository<User> userRepository,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	AuthTokenFactory tokenFactory
) : ICommandHandler<RefreshTokensCommand, AuthTokensResult>
{
	public async Task<AuthTokensResult> HandleAsync(RefreshTokensCommand command, CancellationToken ct)
	{
		var session = await sessionRepository.FindByRefreshTokenAsync(command.RefreshToken);
		if (session == null)
		{
			await sessionRepository.TryTerminateSessionByUsedRefreshTokenAsync(command.RefreshToken);
			throw new InvalidTokenException();
		}

		var user = await userRepository.FirstOrDefaultAsync(
			user => user.Id == session.UserId,
			user => new TokenUser(user.Id, user.Login),
			ct
		);
		if (user == null)
		{
			await sessionRepository.TerminateSessionAsync(session.Id);
			throw new InvalidTokenException();
		}

		var now = DateTime.UtcNow;
		session.LastSeen = now;
		session.ExpiresAt = tokenFactory.CalculateExpiresAt(session.CreatedAt, now);

		if (session.ExpiresAt <= now)
		{
			await sessionRepository.TerminateSessionAsync(session.Id);
			throw new InvalidTokenException();
		}

		var sessionTtl = session.ExpiresAt - now;
		var usedRefreshTokenTtl = session.CreatedAt.Add(tokenFactory.AbsoluteSessionLifetime) - now;
		var newRefreshToken = jwtHelper.GenerateRefreshToken();

		if (
			!await sessionRepository.TryRotateRefreshAsync(
				session,
				command.RefreshToken,
				newRefreshToken,
				sessionTtl,
				usedRefreshTokenTtl
			)
		)
		{
			await sessionRepository.TryTerminateSessionByUsedRefreshTokenAsync(command.RefreshToken);
			throw new InvalidTokenException();
		}

		return tokenFactory.CreateAuthTokens(user, session, newRefreshToken, now);
	}
}
