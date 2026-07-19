namespace ViaTradeBackend.Contracts.Users;

public record UserMeResponse(
	int Id,
	string Login,
	DateTime LastLoginDate,
	DateTime RegisterDate,
	string? TelegramId
);

