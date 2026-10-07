using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.Create;

public sealed record CreateReminderCommand(int InstrumentId, string Text, DateTime RemindAt) : ICommand<ReminderResult>;
