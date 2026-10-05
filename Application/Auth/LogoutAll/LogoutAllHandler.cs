using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Auth.LogoutAll;

public sealed class LogoutAllHandler(ISessionRepository sessionRepository) : ICommandHandler<LogoutAllCommand>
{
	public async Task HandleAsync(LogoutAllCommand command, CancellationToken ct = default)
	{
		var sessions = await sessionRepository.ListByUserAsync(command.UserId);

		foreach (var session in sessions)
			await sessionRepository.TerminateSessionAsync(session.Id);
	}
}
