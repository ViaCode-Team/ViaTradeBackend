using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Users;

public record LinkTelegramRequest(
	[StringLength(256, MinimumLength = 1)] string TelegramToken,
	[StringLength(64, MinimumLength = 1)] string TelegramId
);
