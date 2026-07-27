using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Instruments;

namespace ViaTradeBackend.Contracts.Reminders;

public record ReminderResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(1024, MinimumLength = 1)] string Text,
	DateTime RemindAt,
	InstrumentBriefResponse? Instrument,
	[Range(1, int.MaxValue)] int UserId
);
