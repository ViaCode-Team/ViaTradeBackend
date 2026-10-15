using System.Text;
using FluentValidation;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Email;

namespace ViaTrade.Application.Auth.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
	public RegisterCommandValidator()
	{
		RuleFor(request => request.Login).RequiredText(1, 64);
		RuleFor(request => request.Login)
			.Must(login => login != null && !login.Contains('@'))
			.WithMessage("Login must not contain @.");
		RuleFor(request => request.Email)
			.NotEmpty()
			.Must(email => email != null && EmailIdentity.IsEmail(email.Trim()))
			.WithMessage("A valid email address is required.");
		RuleFor(request => request.Password).RequiredText(8, 72);
		RuleFor(request => request.Password)
			.Must(password => password != null && Encoding.UTF8.GetByteCount(password) <= 72)
			.WithMessage("Password must not exceed 72 UTF-8 bytes.");
	}
}
