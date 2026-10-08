using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Users.LinkTelegram;

public sealed class LinkTelegramCommandValidator : AbstractValidator<LinkTelegramCommand>
{
	public LinkTelegramCommandValidator()
	{
		RuleFor(request => request.TelegramToken).RequiredText(1, 256);
		RuleFor(request => request.TelegramId).RequiredText(1, 64);
	}
}
