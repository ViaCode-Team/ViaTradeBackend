using Mediator;

namespace ViaTrade.Application.Reminders.Update;

public sealed record UpdateReminderCommand(int ReminderId, string Text, DateTime RemindAt) : ICommand;
