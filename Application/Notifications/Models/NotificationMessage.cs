namespace ViaTrade.Application.Notifications.Models;

public sealed record NotificationMessage(
	string NotificationId,
	string Type,
	int UserId,
	string ChatId,
	string Payload,
	DateTimeOffset CreatedAt
);
