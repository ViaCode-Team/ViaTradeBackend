namespace ViaTradeBackend.Contracts.Users;

public record UserMeResponse(int Id, string Login, DateTime LastLoginAt, DateTime RegisteredAt, string? TelegramId);
