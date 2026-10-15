using Mediator;
using ViaTrade.Application.Email;

namespace ViaTrade.Application.Auth.Register;

public sealed class RegisterHandler(EmailSubmissionService submission)
	: ICommandHandler<RegisterCommand, EmailAcceptedResult>
{
	public async ValueTask<EmailAcceptedResult> Handle(RegisterCommand command, CancellationToken ct)
	{
		return await submission.RegisterAsync(command.Login, command.Email, command.Password, ct);
	}
}
