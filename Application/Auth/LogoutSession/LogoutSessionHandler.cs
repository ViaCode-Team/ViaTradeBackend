using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Auth.LogoutSession;

public sealed class LogoutSessionHandler(ISessionRepository sessionRepository) : ICommandHandler<LogoutSessionCommand>
{
	public async Task HandleAsync(LogoutSessionCommand command, CancellationToken ct = default)
	{
		await sessionRepository.TerminateSessionAsync(command.SessionId);
	}
}
