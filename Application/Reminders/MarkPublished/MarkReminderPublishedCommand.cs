using Mediator;

namespace ViaTrade.Application.Reminders.MarkPublished;

public sealed record MarkReminderPublishedCommand(int UserId, int ReminderId) : ICommand<PublishReminderResult>;
