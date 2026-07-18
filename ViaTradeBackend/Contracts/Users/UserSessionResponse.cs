namespace ViaTradeBackend.Contracts.Users;

public record UserSessionResponse(
	string Id,
	int UserId,
	string UserAgent,
	DateTime CreatedAt,
	DateTime LastSeen
);

