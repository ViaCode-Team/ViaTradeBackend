using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Users.Interfaces;
using MediatR;

namespace Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : ICommand<AuthInternalResult>;

public class RefreshTokenCommandHandler(
	IUserRepository userRepository, IJwtHelper jwtHelper, ISessionRepository sessionRepository, IRefreshTokenRepository refreshTokenRepository)
	: IRequestHandler<RefreshTokenCommand, AuthInternalResult>
{
	private readonly TimeSpan _sessionTtl = TimeSpan.FromDays(7);

	public async Task<AuthInternalResult> Handle(RefreshTokenCommand request, CancellationToken ct)
	{
		var sessionId = await refreshTokenRepository.GetSessionIdAsync(request.RefreshToken)
			?? throw new UnauthorizedAccessException();

		var session = await sessionRepository.GetAsync(sessionId)
			?? throw new UnauthorizedAccessException();

		var user = await userRepository.GetByIdAsync(session.UserId, ct)
			?? throw new UnauthorizedAccessException();

		session.LastSeen = DateTime.UtcNow;
		await sessionRepository.CreateAsync(session, _sessionTtl);

		var newAccessToken = jwtHelper.GenerateAccessToken(user, sessionId);
		var newRefreshToken = jwtHelper.GenerateRefreshToken();

		await refreshTokenRepository.RotateAsync(sessionId, newRefreshToken, _sessionTtl);

		return new AuthInternalResult
		{
			AccessToken = newAccessToken,
			RefreshToken = newRefreshToken
		};
	}
}
