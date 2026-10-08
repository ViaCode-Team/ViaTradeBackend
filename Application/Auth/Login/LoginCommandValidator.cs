using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
	public LoginCommandValidator()
	{
		RuleFor(request => request.Login).RequiredText(1, 64);
		RuleFor(request => request.Password).RequiredText(8, 72);
	}
}
