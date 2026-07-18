using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Commands;

public record LogoutSessionCommand(string RefreshToken) : ICommandWithoutUoW;

public class LogoutSessionCommandHandler(
	ISessionRepository sessionRepository,
	IRefreshTokenRepository refreshTokenRepository)
	: IRequestHandler<LogoutSessionCommand>
{
	private readonly ISessionRepository _sessionRepository = sessionRepository;
	private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

	public async Task Handle(LogoutSessionCommand request, CancellationToken cancellationToken)
	{
		var sessionId = await _refreshTokenRepository.GetSessionIdAsync(request.RefreshToken);
		if (sessionId == null)
			return;

		await _refreshTokenRepository.RemoveAsync(sessionId);
		await _sessionRepository.RemoveAsync(sessionId);
	}
}
