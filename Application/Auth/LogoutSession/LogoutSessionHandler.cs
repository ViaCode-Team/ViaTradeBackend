using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;

namespace ViaTrade.Application.Auth.LogoutSession;

public sealed class LogoutSessionHandler(IUserContext userContext, ISessionRepository sessionRepository)
	: IVoidCommandHandler<LogoutSessionCommand>
{
	public async ValueTask Handle(LogoutSessionCommand command, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var session = await sessionRepository.FindByIdAsync(userContext.SessionId);
		if (session == null)
			return;

		if (session.UserId != userContext.UserId)
			throw new InvalidTokenException("The session does not belong to the current user.");

		await sessionRepository.TerminateSessionAsync(userContext.SessionId);
	}
}
