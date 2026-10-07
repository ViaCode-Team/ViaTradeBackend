using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Reminders.Delete;

public sealed record DeleteReminderCommand(int ReminderId) : ICommand;
