using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Commands;

public record LogoutSessionCommand(string RefreshToken) : ICommand;

public class LogoutSessionCommandHandler(
	ISessionRepository sessionRepository, IRefreshTokenRepository refreshTokenRepository)
	: IRequestHandler<LogoutSessionCommand>
{
	public async Task Handle(LogoutSessionCommand request, CancellationToken cancellationToken)
	{
		var sessionId = await refreshTokenRepository.GetSessionIdAsync(request.RefreshToken);
		if (sessionId == null)
			return;

		await refreshTokenRepository.RemoveAsync(sessionId);
		await sessionRepository.RemoveAsync(sessionId);
	}
}
