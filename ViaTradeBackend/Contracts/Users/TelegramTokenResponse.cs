using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Users;

public record TelegramTokenResponse([StringLength(256)] string TelegramToken);
