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
