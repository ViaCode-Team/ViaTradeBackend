using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Users.CreateTelegramLink;

public sealed class CreateTelegramLinkCommandValidator : UserRequestValidator<CreateTelegramLinkCommand>
{
	public CreateTelegramLinkCommandValidator()
		: base(request => request.UserId) { }
}
