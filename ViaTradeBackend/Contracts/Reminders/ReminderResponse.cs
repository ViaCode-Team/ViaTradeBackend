using ViaTradeBackend.Contracts.Trades;

namespace ViaTradeBackend.Contracts.Reminders;

public record ReminderResponse(int Id, string Text, DateTime DateTime, TradeCodeBriefResponse? TradeCode, int UserId);
