using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Auth.LogoutAll;

public sealed class LogoutAllCommandValidator : UserRequestValidator<LogoutAllCommand>
{
	public LogoutAllCommandValidator()
		: base(request => request.UserId) { }
}
