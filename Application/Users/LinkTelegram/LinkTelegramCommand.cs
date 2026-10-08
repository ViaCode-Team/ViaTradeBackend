using Mediator;

namespace ViaTrade.Application.Users.LinkTelegram;

public sealed record LinkTelegramCommand(string TelegramToken, string TelegramId) : ICommand;
