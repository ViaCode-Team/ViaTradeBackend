using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Users;

public record TelegramTokenResponse([StringLength(256)] string TelegramToken);
