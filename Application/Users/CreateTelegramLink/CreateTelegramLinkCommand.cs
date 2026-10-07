using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Users.CreateTelegramLink;

public sealed record CreateTelegramLinkCommand() : ICommand<TelegramLinkResult>;
