using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Users;

public record UserMeResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(64)] string Login,
	DateTime LastLoginAt,
	DateTime RegisteredAt,
	[StringLength(64)] string? TelegramId
);
