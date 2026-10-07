using Mediator;

namespace ViaTrade.Application.Users.CreateTelegramLink;

public sealed record CreateTelegramLinkCommand() : ICommand<TelegramLinkResult>;
