using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Users;

public record UserTelegramResponse([Range(1, int.MaxValue)] int Id, [StringLength(64)] string TelegramId);
