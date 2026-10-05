using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Reminders.Update;

public sealed record UpdateReminderCommand(int UserId, int ReminderId, string Text, DateTime RemindAt) : ICommand;
