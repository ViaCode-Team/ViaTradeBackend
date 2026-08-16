using System.ComponentModel.DataAnnotations;
using ViaTrade.Api.Contracts.Instruments;

namespace ViaTrade.Api.Contracts.Reminders;

public record ReminderResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(1024, MinimumLength = 1)] string Text,
	DateTime RemindAt,
	InstrumentBriefResponse? Instrument,
	DateTime? DeliveredAt
);
