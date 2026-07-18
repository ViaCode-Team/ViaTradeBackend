using Application.Auth.Interfaces;
using MediatR;

namespace Application.Auth.Commands;

public record LogoutAllCommand(int UserId) : IRequest;

public class LogoutAllCommandHandler(
	ISessionRepository sessionRepository,
	IRefreshTokenRepository refreshTokenRepository)
	: IRequestHandler<LogoutAllCommand>
{
	private readonly ISessionRepository _sessionRepository = sessionRepository;
	private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

	public async Task Handle(LogoutAllCommand request, CancellationToken cancellationToken)
	{
		var sessions = await _sessionRepository.GetUserSessionsAsync(request.UserId);

		foreach (var session in sessions)
		{
			await _refreshTokenRepository.RemoveAsync(session.Id);
			await _sessionRepository.RemoveAsync(session.Id);
		}
	}
}
