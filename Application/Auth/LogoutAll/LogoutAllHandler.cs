using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Auth.LogoutAll;

public sealed class LogoutAllHandler(IUserContext userContext, ISessionRepository sessionRepository)
	: IVoidCommandHandler<LogoutAllCommand>
{
	public async ValueTask Handle(LogoutAllCommand command, CancellationToken ct)
	{
		var sessions = await sessionRepository.ListByUserAsync(userContext.UserId);

		foreach (var session in sessions)
			await sessionRepository.TerminateSessionAsync(session.Id);
	}
}
