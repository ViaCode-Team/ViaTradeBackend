using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Users.ConsumeTelegramToken;

public sealed record ConsumeTelegramTokenCommand(string TelegramToken) : ICommand<TelegramTokenResult?>;
