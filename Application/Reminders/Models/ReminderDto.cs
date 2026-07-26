using Application.Notes.Models;

namespace Application.Reminders.Models;

public record ReminderDto(int Id, string Text, DateTime RemindAt, InstrumentBriefDto? Instrument, int UserId);

public record ReminderProjectionDto(
	int Id,
	string Text,
	DateTime RemindAt,
	int TradeCodeId,
	string TradeCodeTicker,
	string? TradeCodeName,
	int UserId
);
