using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Commands;

public record LogoutAllCommand(int UserId) : ICommand;

public class LogoutAllCommandHandler(
	ISessionRepository sessionRepository, IRefreshTokenRepository refreshTokenRepository)
	: IRequestHandler<LogoutAllCommand>
{
	public async Task Handle(LogoutAllCommand request, CancellationToken cancellationToken)
	{
		var sessions = await sessionRepository.GetUserSessionsAsync(request.UserId);

		foreach (var session in sessions)
		{
			await refreshTokenRepository.RemoveAsync(session.Id);
			await sessionRepository.RemoveAsync(session.Id);
		}
	}
}
