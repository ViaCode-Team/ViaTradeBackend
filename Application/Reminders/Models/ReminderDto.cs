using ViaTrade.Application.Notes.Models;

namespace ViaTrade.Application.Reminders.Models;

public record ReminderDto(
	int Id,
	string Text,
	DateTime RemindAt,
	InstrumentBriefDto? Instrument,
	int UserId,
	string TelegramId,
	DateTime? DeliveredAt
);

public record ReminderProjectionDto(
	int Id,
	string Text,
	DateTime RemindAt,
	int InstrumentId,
	string InstrumentTicker,
	string? InstrumentName,
	int UserId,
	DateTime? DeliveredAt
);
