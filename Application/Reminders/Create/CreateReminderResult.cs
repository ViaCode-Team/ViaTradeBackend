namespace ViaTrade.Application.Reminders.Create;

public sealed record CreateReminderResult(int Id, string Text, DateTime RemindAt, DateTime? DeliveredAt);
