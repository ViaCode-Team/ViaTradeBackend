using ViaTradeBackend.Contracts.Instruments;

namespace ViaTradeBackend.Contracts.Reminders;

public record ReminderResponse(int Id, string Text, DateTime RemindAt, InstrumentBriefResponse? Instrument, int UserId);
