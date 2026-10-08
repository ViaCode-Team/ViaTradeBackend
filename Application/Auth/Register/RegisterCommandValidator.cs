using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Auth.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
	public RegisterCommandValidator()
	{
		RuleFor(request => request.Login).RequiredText(1, 64);
		RuleFor(request => request.Password).RequiredText(8, 72);
	}
}
