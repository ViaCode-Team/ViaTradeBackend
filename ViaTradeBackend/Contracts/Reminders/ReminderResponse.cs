namespace ViaTradeBackend.Contracts.Reminders;

public record ReminderResponse(
	int Id,
	string Text,
	DateTime DateTime,
	int TradeCodeId,
	int UserId
);

