using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Reminders.MarkDelivered;

public sealed record MarkReminderDeliveredCommand(int UserId, int ReminderId) : ICommand;
