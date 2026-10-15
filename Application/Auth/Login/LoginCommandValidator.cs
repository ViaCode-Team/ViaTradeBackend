using System.Text;
using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
	public LoginCommandValidator()
	{
		RuleFor(request => request.Login).RequiredText(1, 254);
		RuleFor(request => request.Password).RequiredText(8, 72);
		RuleFor(request => request.Password)
			.Must(password => password != null && Encoding.UTF8.GetByteCount(password) <= 72)
			.WithMessage("Password must not exceed 72 UTF-8 bytes.");
	}
}
