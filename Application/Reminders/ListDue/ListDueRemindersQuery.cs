using Mediator;

namespace ViaTrade.Application.Reminders.ListDue;

public sealed record ListDueRemindersQuery(int Limit) : IQuery<IReadOnlyList<DueReminderResult>>;
