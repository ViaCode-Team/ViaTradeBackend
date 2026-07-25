using Application.Notes.Models;

namespace Application.Reminders.Models;

public record ReminderDto(int Id, string Text, DateTime DateTime, TradeCodeBriefDto? TradeCode, int UserId);

public record ReminderProjectionDto(
	int Id,
	string Text,
	DateTime DateTime,
	int TradeCodeId,
	string TradeCodeTicker,
	string? TradeCodeName,
	int UserId
);
